using System;
using ADLMidi.NET;
using UAlbion.Core;
using UAlbion.Formats.Assets;

namespace UAlbion.Game;

public class AlbionMusicGenerator : Component, IAudioGenerator
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

        var assets = Resolve<IAssetManager>();
        var xmiBytes = assets.LoadSong(SongId);
        if ((xmiBytes?.Length ?? 0) == 0)
            return;

        var banks = assets.LoadSoundBanks();
        if(banks == null)
        {
            Error("AlbionMusicGenerator: Could not load sound banks");
            return;
        }

        _player.OpenBankData(banks);
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