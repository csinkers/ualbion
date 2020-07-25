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
using UAlbion.Formats.Config;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;

namespace UAlbion.Game.Veldrid.Audio
{
    public class AudioManager : ServiceComponent<IAudioManager>, IAudioManager
    {
        readonly bool _standalone;
        readonly IDictionary<SampleId, AudioBuffer> _sampleCache = new Dictionary<SampleId, AudioBuffer>();
        readonly IDictionary<(SongId, int), AudioBuffer> _waveLibCache = new Dictionary<(SongId, int), AudioBuffer>();
        readonly IList<ActiveSound> _activeSounds = new List<ActiveSound>();
        readonly ManualResetEvent _doneEvent = new ManualResetEvent(false);
        readonly object _syncRoot = new object();

        SongId? _activeSongId;
        StreamingAudioSource _music;
        AlbionMusicGenerator _musicGenerator;
        AmbientSoundPlayer _ambientPlayer;

        public AudioManager(bool standalone)
        {
            On<SoundEvent>(Play);
            On<WaveLibEvent>(PlayWaveLib);
            On<SongEvent>(e => PlayMusic(e.SongId));
            On<AmbientEvent>(e => PlayAmbient(e.SongId));
            On<MuteEvent>(e => StopAll());
            On<QuitEvent>(e => _doneEvent.Set());

            _standalone = standalone;
        }

        protected override void Subscribed()
        {
            _doneEvent.Reset();
            Task.Run(AudioThread);
            base.Subscribed();
        }

        protected override void Unsubscribed()
        {
            _doneEvent.Set();
            base.Subscribed();
        }

        class ActiveSound
        {
            public ActiveSound(AudioSource source, object id, int restartProbability)
            {
                Source = source;
                Id = id;
                RestartProbability = restartProbability;
            }

            public AudioSource Source { get; }
            public object Id { get; }
            public int RestartProbability { get; }
        }

        AudioBuffer GetBuffer(SampleId id)
        {
            lock (_syncRoot)
            {
                if (_sampleCache.TryGetValue(id, out var buffer))
                    return buffer;
                var assets = Resolve<IAssetManager>();
                var sample = assets.LoadSample(id);
                if (sample == null)
                {
                    Raise(new LogEvent(LogEvent.Level.Error, $"Could not load audio sample {(int)id}: {id}"));
                    _sampleCache[id] = null;
                    return null;
                }

                buffer = new AudioBufferUInt8(sample.Samples, sample.SampleRate);
                _sampleCache[id] = buffer;
                return buffer;
            }
        }

        AudioBuffer GetBuffer(SongId songId, int instrument)
        {
            var key = (songId, instrument);
            lock (_syncRoot)
            {
                if (_waveLibCache.TryGetValue(key, out var buffer))
                    return buffer;
                var assets = Resolve<IAssetManager>();
                var sample = assets.LoadWaveLib(songId, instrument);
                if (sample == null)
                {
                    Raise(new LogEvent(LogEvent.Level.Error, $"Could not load audio sample {key}"));
                    _waveLibCache[key] = null;
                    return null;
                }

                buffer = new AudioBufferUInt8(sample.Samples, sample.SampleRate);
                _waveLibCache[key] = buffer;
                return buffer;
            }
        }

        void PlayWaveLib(WaveLibEvent e)
        {
            var buffer = GetBuffer(e.SongId, e.Instrument);
            if (buffer == null)
                return;

            var source = new SimpleAudioSource(buffer)
            {
                Volume = e.Velocity == 0 ? 0.4f : 0.4f * (e.Velocity / 255.0f),
                SourceRelative = true
            };

            var active = new ActiveSound(source, (e.SongId, e.Instrument), 0);
            active.Source.Play();
            lock (_syncRoot)
                _activeSounds.Add(active);
        }

        void Play(SoundEvent e)
        {
            if (e.Mode == SoundEvent.SoundMode.Silent)
                return;

            var buffer = GetBuffer(e.SoundId);
            if (buffer == null)
                return;

            var context = Resolve<IEventManager>().Context;
            var map = Resolve<IMapManager>()?.Current;
            var tileSize = map?.TileSize ?? Vector3.One;
            var mapSource = context.Source as EventSource.Map;
            var source = new SimpleAudioSource(buffer)
            {
                Volume = e.Volume == 0 ? 1.0f : e.Volume / 255.0f,
                Looping = e.Mode == SoundEvent.SoundMode.LocalLoop,
                Position = tileSize * new Vector3(mapSource?.X ?? 0, mapSource?.Y ?? 0, 0.0f),
                SourceRelative = mapSource == null, // If we couldn't localise the sound then play it at (0,0) relative to the player.
                ReferenceDistance = 1.0f * tileSize.X,
                RolloffFactor = 4.0f
            };

            if (e.FrequencyOverride != 0)
                source.Pitch = (float)e.FrequencyOverride / buffer.SamplingRate;

            var active = new ActiveSound(
                source,
                e.SoundId,
                e.RestartProbability);

            active.Source.Play();
            lock (_syncRoot)
                _activeSounds.Add(active);
        }

        void PlayMusic(SongId songId)
        {
            if (songId == _activeSongId)
                return;

            lock (_syncRoot)
            {
                StopMusic();

                _musicGenerator = AttachChild(new AlbionMusicGenerator(songId));

                _music = new StreamingAudioSource(_musicGenerator)
                {
                    Volume = 1.0f,
                    Looping = false, // Looping is the responsibility of the generator
                    SourceRelative = true,
                    Position = Vector3.Zero
                };

                _music.Play();
                _activeSongId = songId;
            }
        }

        void PlayAmbient(SongId songId)
        {
            lock (_syncRoot)
            {
                StopAmbient();
                _ambientPlayer = AttachChild(new AmbientSoundPlayer(songId));
            }
        }

        void StopAmbient() => _ambientPlayer?.Remove();

        void StopMusic()
        {
            if (_music == null)
                return;

            _music.Stop();
            _music.Dispose();
            _musicGenerator.Remove();
            _music = null;
            _musicGenerator = null;
            _activeSongId = null;
        }

        void StopAll()
        {
            lock (_syncRoot)
            {
                StopAmbient();
                StopMusic();
                foreach (var sound in _activeSounds)
                {
                    sound.Source.Stop();
                    sound.Source.Dispose();
                }
                _activeSounds.Clear();

                foreach (var sample in _sampleCache.Values)
                    sample?.Dispose();
                _sampleCache.Clear();
            }
        }

        void AudioThread()
        {
            using var device = new AudioDevice { DistanceModel = DistanceModel.InverseDistance };
            var config = Resolve<GameConfig>();

            while (!_doneEvent.WaitOne((int)(config.Audio.AudioPollIntervalSeconds * 1000)))
            {
                if (_standalone)
                    Raise(new BeginFrameEvent());

                lock (_syncRoot) // Reap any dead sounds and update any streaming sources
                {
                    for (int i = 0; i < _activeSounds.Count;)
                    {
                        var sound = _activeSounds[i];
                        if (!sound.Source.Looping && sound.Source.State == SourceState.Stopped)
                        {
                            sound.Source.Dispose();
                            _activeSounds.RemoveAt(i);
                        }
                        else
                        {
                            if (sound.Source is StreamingAudioSource stream)
                                stream.CycleBuffers();

                            i++;
                        }
                    }

                    _music?.CycleBuffers();
                }

                var camera = Resolve<ICamera>();
                if (camera != null)
                    device.Listener.Position = camera.Position;
            }

            StopAll();
        }

        public IList<string> ActiveSounds
        {
            get
            {
                var position = Resolve<ICamera>()?.Position ?? Vector3.Zero;
                lock (_syncRoot)
                {
                    return _activeSounds
                        .Select(x =>
                        {
                            var distance = (x.Source.Position - position).Length();
                            return
                                $"{x.Id} {x.Source.Looping} {x.Source.State} {x.Source.Volume} {x.Source.Pitch} {distance} -> {x.Source.Position}";
                        })
                        .ToList();
                }
            }
        }
    }
}
