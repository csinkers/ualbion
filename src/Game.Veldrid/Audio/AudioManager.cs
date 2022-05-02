using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Audio;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Events;

namespace UAlbion.Game.Veldrid.Audio;

public sealed class AudioManager : ServiceComponent<IAudioManager>, IAudioManager, IDisposable
{
    const int DefaultSampleRate = 11025;
    readonly bool _standalone;
    readonly IDictionary<SampleId, AudioBuffer> _sampleCache = new Dictionary<SampleId, AudioBuffer>();
    readonly IDictionary<(SongId, int), AudioBuffer> _waveLibCache = new Dictionary<(SongId, int), AudioBuffer>();
    readonly IList<ActiveSound> _activeSounds = new List<ActiveSound>();
    readonly ManualResetEvent _doneEvent = new(false);
    readonly object _syncRoot = new();

    StreamingAudioSource _music;
    AlbionMusicGenerator _musicGenerator;
    AmbientSoundPlayer _ambientPlayer;

    public AudioManager(bool standalone)
    {
        On<SoundEvent>(Play);
        On<SoundEffectEvent>(Play);
        On<WaveLibEvent>(PlayWaveLib);
        On<SongEvent>(e => PlayMusic(e.SongId));
        On<AmbientEvent>(e => PlayAmbient(e.SongId));
        On<MuteEvent>(_ => StopAll());
        On<QuitEvent>(_ => _doneEvent.Set());

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
                Error($"Could not load audio sample {id.Id}: {id}");
                _sampleCache[id] = null;
                return null;
            }

            var sampleRate = sample.SampleRate == -1 ? DefaultSampleRate : sample.SampleRate;
            buffer = new AudioBufferUInt8(sample.Samples, sampleRate);
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
            var sample = assets.LoadWaveLib(songId.ToWaveLibrary())?[instrument];
            if (sample == null)
            {
                Error($"Could not load audio sample {key}");
                _waveLibCache[key] = null;
                return null;
            }

            var sampleRate = sample.SampleRate == -1 ? DefaultSampleRate : sample.SampleRate;
            buffer = new AudioBufferUInt8(sample.Samples, sampleRate);
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

    void Play(ISoundEvent e)
    {
        if (e.Mode == SoundMode.Silent)
            return;

        var buffer = GetBuffer(e.SoundId);
        if (buffer == null)
            return;

        var context = Resolve<IEventManager>().Context;
        var map = Resolve<IMapManager>()?.Current;
        var tileSize = map?.TileSize ?? Vector3.One;
        var source = new SimpleAudioSource(buffer)
        {
            Volume = e.Volume == 0 ? 1.0f : e.Volume / 255.0f,
            Looping = e.Mode == SoundMode.LocalLoop,
            Position = tileSize * new Vector3(context.Source.X, context.Source.Y, 0.0f),
            SourceRelative = context.Source.AssetId.Type != AssetType.Map, // If we couldn't localise the sound then play it at (0,0) relative to the player.
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
        if (_musicGenerator?.SongId == songId)
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

    void StopAmbient()
    {
        Info($"Stopping ambient playback ({_ambientPlayer?.SongId})");
        _ambientPlayer?.Remove();
    }

    void StopMusic()
    {
        if (_music == null)
            return;

        Info($"Stopping music playback of {_musicGenerator.SongId}");
        _music.Stop();
        _music.Dispose();
        _musicGenerator.Remove();
        _music = null;
        _musicGenerator = null;
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
        var config = Resolve<IGameConfigProvider>().Game;

        while (!_doneEvent.WaitOne((int)(config.Audio.AudioPollIntervalSeconds * 1000)))
        {
            if (_standalone)
                Raise(BeginFrameEvent.Instance);

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

    public void Dispose()
    {
        _doneEvent?.Dispose();
        _music?.Dispose();
    }
}