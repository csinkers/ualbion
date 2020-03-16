using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Audio;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Veldrid.Audio
{
    public class AudioManager : Component, IAudioManager
    {
        readonly bool _standalone;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<AudioManager, QuitEvent>((x,e) => x._doneEvent.Set()),
            H<AudioManager, SoundEvent>((x,e) => x.Play(e))
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

            var source = new AudioSource(buffer)
            {
                Volume = e.Volume / 255.0f,
                Looping = e.Mode == SoundEvent.SoundMode.LocalLoop
            };

            /*
            e.FrequencyOverride == 0 
                ? buffer.SamplingRate 
                : e.FrequencyOverride
            */

            var active = new ActiveSound(
                source,
                e.SoundId,
                e.RestartProbability);

            active.Source.Play();
            _activeSounds.Add(active);
        }

        public AudioManager(bool standalone) : base(Handlers) => _standalone = standalone;
        public override void Subscribed()
        {
            Task.Run(AudioThread);
            base.Subscribed();
        }

        void AudioThread()
        {
            using var d = new AudioDevice();
            if (_standalone)
            {
                while (!_doneEvent.WaitOne(500))
                {
                    Raise(new BeginFrameEvent());
                }
            }
            else _doneEvent.WaitOne();

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
    }
}