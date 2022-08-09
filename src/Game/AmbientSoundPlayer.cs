using System;
using ADLMidi.NET;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using UAlbion.Formats;
using UAlbion.Formats.Ids;
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

        var soundBanks = assets.LoadSoundBanks();
        if (soundBanks is not GlobalTimbreLibrary timbreLibrary)
        {
            Error("AlbionMusicGenerator: Could not load sound banks");
            return;
        }

        var wopl = new WoplFile(timbreLibrary);
        var woplBytes = wopl.GetRawWoplBytes(ApiUtil.Assert);

        _player = AdlMidi.Init();
        _player.SetNoteHook(_hook, IntPtr.Zero);
        _player.OpenBankData(woplBytes);
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