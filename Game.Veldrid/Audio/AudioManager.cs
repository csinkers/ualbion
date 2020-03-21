using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Audio;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;

namespace UAlbion.Game.Veldrid.Audio
{
    public class AudioManager : Component, IAudioManager
    {
        readonly bool _standalone;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<AudioManager, QuitEvent>((x,e) => x._doneEvent.Set()),
            H<AudioManager, SoundEvent>((x,e) => x.Play(e)),
            H<AudioManager, MuteEvent>((x,e) => x.StopAll())
        );

        readonly IDictionary<SampleId, AudioBuffer> _sampleCache = new Dictionary<SampleId, AudioBuffer>();
        readonly IList<ActiveSound> _activeSounds = new List<ActiveSound>();
        readonly ManualResetEvent _doneEvent = new ManualResetEvent(false);
        readonly object _syncRoot = new object();

        class ActiveSound
        {
            public ActiveSound(AudioSource source, SampleId id, int restartProbability)
            {
                Source = source;
                Id = id;
                RestartProbability = restartProbability;
            }

            public AudioSource Source { get; }
            public SampleId Id { get; }
            public int RestartProbability { get; }
        }

        AudioBuffer GetBuffer(SampleId id)
        {
            lock(_syncRoot)
            {
                if (_sampleCache.TryGetValue(id, out var buffer))
                    return buffer;
                var assets = Resolve<IAssetManager>();
                var sample = assets.LoadSample(id);
                if(sample == null)
                {
                    Raise(new LogEvent(LogEvent.Level.Error, $"Could not load audio sample {(int)id}: {id}"));
                    _sampleCache[id] = null;
                    return null;
                }

                buffer = new AudioBuffer(sample.Samples, sample.SampleRate);
                _sampleCache[id] = buffer;
                return buffer;
            }
        }

        void Play(SoundEvent e)
        {
            if (e.Mode == SoundEvent.SoundMode.Silent)
                return;

            var buffer = GetBuffer(e.SoundId);
            if (buffer == null)
                return;

            var map = Resolve<IMapManager>()?.Current;
            var tileSize = map?.TileSize ?? Vector3.One;
            var source = new AudioSource(buffer)
            {
                Volume = e.Volume == 0 ? 1.0f : e.Volume / 255.0f,
                Looping = e.Mode == SoundEvent.SoundMode.LocalLoop,
                Position = tileSize * new Vector3(e.Context.X, e.Context.Y, 0.0f),
                ReferenceDistance = 4.0f * tileSize.X,
                RolloffFactor = 4.0f
            };

            if (e.FrequencyOverride != 0)
                source.Pitch = (float)e.FrequencyOverride / buffer.SamplingRate;

            var active = new ActiveSound(
                source,
                e.SoundId,
                e.RestartProbability);

            active.Source.Play();
            lock(_syncRoot)
                _activeSounds.Add(active);
        }

        void StopAll()
        {
            lock (_syncRoot)
            {
                foreach (var sound in _activeSounds)
                {
                    sound.Source.Stop();
                    sound.Source.Dispose();
                }
                _activeSounds.Clear();

                foreach(var sample in _sampleCache.Values)
                    sample.Dispose();
                _sampleCache.Clear();
            }
        }

        public AudioManager(bool standalone) : base(Handlers) => _standalone = standalone;
        public override void Subscribed()
        {
            Task.Run(AudioThread);
            base.Subscribed();
        }

        const int AudioPollIntervalMs = 100;
        void AudioThread()
        {
            using var device = new AudioDevice { DistanceModel = DistanceModel.InverseDistance };

            while (!_doneEvent.WaitOne(AudioPollIntervalMs))
            {
                if (_standalone)
                    Raise(new BeginFrameEvent());

                lock (_syncRoot) // Reap any dead sounds
                {
                    for (int i = 0; i < _activeSounds.Count;)
                    {
                        var sound = _activeSounds[i];
                        if (!sound.Source.Looping && sound.Source.State == SourceState.Stopped)
                        {
                            sound.Source.Dispose();
                            _activeSounds.RemoveAt(i);
                        }
                        else i++;
                    }
                }

                var camera = Resolve<ICamera>();
                if (camera != null)
                    device.Listener.Position = camera.Position;
            }

            lock (_syncRoot)
            {
                foreach (var sound in _activeSounds)
                {
                    sound.Source.Stop();
                    sound.Source.Dispose();
                }

                foreach (var buffer in _sampleCache.Values)
                    buffer.Dispose();

                _activeSounds.Clear();
                _sampleCache.Clear();
            }
        }

        public IList<string> ActiveSounds
        {
            get
            {
                var position = Resolve<ICamera>()?.Position ?? Vector3.Zero;
                lock (_syncRoot)
                    return _activeSounds
                        .Select(x =>
                        {
                            var distance = (x.Source.Position - position).Length();
                            return $"{x.Id} {x.Source.Looping} {x.Source.State} {x.Source.Volume} {x.Source.Pitch} {distance} -> {x.Source.Position}";
                        })
                        .ToList();
            }
        }
    }
}