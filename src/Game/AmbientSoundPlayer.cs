using System;
using ADLMidi.NET;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;

namespace UAlbion.Game;

public class AmbientSoundPlayer : Component
{
    readonly NoteHook _hook;
    MidiPlayer _player;

    public SongId SongId { get; }

    public AmbientSoundPlayer(SongId songId)
    {
        On<EngineUpdateEvent>(e => Tick(e.DeltaSeconds));

        SongId = songId;
        _hook = NoteHook;
    }

    protected override void Subscribed()
    {
        if (_player != null)
            return;

        var assets = Resolve<IAssetManager>();
        var xmiBytes = assets.LoadSong(SongId);
        if ((xmiBytes?.Length ?? 0) == 0)
            return;

        _player = AdlMidi.Init();
        _player.SetNoteHook(_hook, IntPtr.Zero);
        _player.OpenBankData(assets.LoadSoundBanks());
        _player.OpenData(xmiBytes);
        _player.SetLoopEnabled(true);
    }

    protected override void Unsubscribed()
    {
        _player?.Dispose();
        _player = null;
    }

    void NoteHook(IntPtr userData, int adlChannel, int note, int instrument, int pressure, double bend) 
        => Raise(new WaveLibEvent(SongId, instrument, pressure, note));

    void Tick(float deltaSeconds)
    {
        const double minTick = 1.0 / 100;
        _player?.TickEvents(deltaSeconds, minTick);
    }
}