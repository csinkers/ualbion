﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Silk.NET.OpenAL;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Config.Properties;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Audio;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Events;

namespace UAlbion.Game.Veldrid.Audio;

public sealed class AudioManager : GameServiceComponent<IAudioManager>, IAudioManager, IDisposable
{
    sealed record ActiveSound(AudioSource Source, object Id, int RestartProbability);

    const int DefaultSampleRate = 11025;
    public static readonly AssetIdAssetProperty<WaveLibraryId> WaveLibProperty = new("WaveLib", WaveLibraryId.None, x => x);

    readonly bool _standalone;
    readonly Dictionary<SampleId, AudioBuffer> _sampleCache = [];
    readonly Dictionary<(SongId, int), AudioBuffer> _waveLibCache = [];
    readonly List<ActiveSound> _activeSounds = [];
    readonly ManualResetEvent _doneEvent = new(false);
    readonly AudioDevice _device;
    readonly Lock _syncRoot = new();

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
        _device = new AudioDevice();
        _device.DistanceModel = DistanceModel.InverseDistance;
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

    AudioBuffer GetBuffer(SampleId id)
    {
        lock (_syncRoot)
        {
            if (_sampleCache.TryGetValue(id, out var buffer))
                return buffer;

            ISample sample = Assets.LoadSample(id);
            if (sample == null)
            {
                Error($"Could not load audio sample {id.Id}: {id}");
                _sampleCache[id] = null;
                return null;
            }

            var sampleRate = sample.SampleRate == -1 ? DefaultSampleRate : sample.SampleRate;
            buffer = _device.CreateBuffer(sample.Samples, sampleRate);
            _sampleCache[id] = buffer;
            return buffer;
        }
    }

    AudioBuffer GetBuffer(SongId songId, int instrument)
    {
        var key = (songId, instrument);
        lock (_syncRoot)
        {
            if (_waveLibCache.TryGetValue(key, out AudioBuffer buffer))
                return buffer;
            var songInfo = Assets.GetAssetInfo(songId);
            var waveLibId = songInfo.GetProperty(WaveLibProperty);
            if (waveLibId.IsNone)
            {
                Info($"Song {songId} has no associated wave library");
                return null;
            }

            var waveLibrary = Assets.LoadWaveLib(waveLibId)?[instrument];
            if (waveLibrary == null)
            {
                Error($"Could not load audio sample {key}");
                _waveLibCache[key] = null;
                return null;
            }

            var sampleRate = waveLibrary.SampleRate == -1 ? DefaultSampleRate : waveLibrary.SampleRate;
            buffer = _device.CreateBuffer(waveLibrary.Samples, sampleRate);
            _waveLibCache[key] = buffer;
            return buffer;
        }
    }

    void PlayWaveLib(WaveLibEvent e)
    {
        var buffer = GetBuffer(e.SongId, e.Instrument);
        if (buffer == null)
            return;

        var source = _device.CreateSource(buffer);
        source.Volume = e.Velocity == 0 ? 0.4f : 0.4f * (e.Velocity / 255.0f);
        source.SourceRelative = true;

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

        var context = (EventContext)Context;
        var map = Resolve<IMapManager>()?.Current;
        var tileSize = map?.TileSize ?? Vector3.One;

        var source = _device.CreateSource(buffer);
        source.Volume = e.Volume == 0 ? 1.0f : e.Volume / 255.0f;
        source.Looping = e.Mode == SoundMode.LocalLoop;
        source.Position = tileSize * new Vector3(context.Source.X, context.Source.Y, 0.0f);
        source.SourceRelative = context.Source.AssetId.Type != AssetType.Map; // If we couldn't localise the sound then play it at (0,0) relative to the player.
        source.ReferenceDistance = 1.0f * tileSize.X;
        source.RolloffFactor = 4.0f;

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

            _music = _device.CreateStreamingSource(_musicGenerator);
            _music.Volume = 1.0f;
            _music.Looping = false; // Looping is the responsibility of the generator
            _music.SourceRelative = true;
            _music.Position = Vector3.Zero;

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
        while (!_doneEvent.WaitOne((int)(ReadVar(V.Game.Audio.PollIntervalSeconds) * 1000)))
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

            var camera = TryResolve<ICameraProvider>()?.Camera;
            if (camera != null)
                _device.Listener.Position = camera.Position;
        }

        StopAll();
    }

    public IList<string> ActiveSounds
    {
        get
        {
            var camera = TryResolve<ICameraProvider>()?.Camera;
            var position = camera?.Position ?? Vector3.Zero;

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
        _device.Dispose();
    }
}
