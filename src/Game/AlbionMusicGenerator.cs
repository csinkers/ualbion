using System;
using ADLMidi.NET;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats.Ids;

namespace UAlbion.Game;

public class AlbionMusicGenerator : GameComponent, IAudioGenerator
{
    MidiPlayer _player;

    public SongId SongId { get; }
    public AlbionMusicGenerator(SongId songId) => SongId = songId;

    protected override void Subscribed()
    {
        if (_player != null)
            return;

        try { _player = AdlMidi.Init(); }
        catch (DllNotFoundException e) { Error($"DLL not found: {e.Message}"); }

        if (_player == null)
            return;

        var xmiBytes = Assets.LoadSong(SongId);
        if ((xmiBytes?.Length ?? 0) == 0)
            return;

        var soundBanks = Assets.LoadSoundBanks();
        if (soundBanks is not GlobalTimbreLibrary timbreLibrary)
        {
            Error("AlbionMusicGenerator: Could not load sound banks");
            return;
        }

        var wopl = new WoplFile(timbreLibrary);
        var woplBytes = wopl.GetRawWoplBytes(ApiUtil.Assert);

        _player.OpenBankData(woplBytes);
        _player.OpenData(xmiBytes);
        _player.SetLoopEnabled(true);
    }

    protected override void Unsubscribed()
    {
        _player?.Close();
        _player = null;
    }

    public int FillBuffer(Span<short> buffer) => _player?.Play(buffer) ?? 0;
}
