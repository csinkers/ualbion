using System;
using ADLMidi.NET;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game
{
    public class AlbionMusicGenerator : Component, IAudioGenerator
    {
        readonly SongId _songId;
        MidiPlayer _player;

        public AlbionMusicGenerator(SongId songId) => _songId = songId;

        protected override void Subscribed()
        {
            if (_player != null)
                return;

            try { _player = AdlMidi.Init(); }
            catch (DllNotFoundException e) { Raise(new LogEvent(LogEvent.Level.Error, $"DLL not found: {e.Message}")); }

            if (_player == null)
                return;

            var assets = Resolve<IAssetManager>();
            var xmiBytes = assets.LoadSong(_songId);
            if ((xmiBytes?.Length ?? 0) == 0)
                return;

            var banks = assets.LoadSoundBanks();
            if(banks == null)
            {
                Raise(new LogEvent(LogEvent.Level.Error, $"AlbionMusicGenerator: Could not load sound banks"));
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
}
