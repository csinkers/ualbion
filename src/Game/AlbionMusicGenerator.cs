using System;
using ADLMidi.NET;
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
            catch (DllNotFoundException) { }

            if (_player == null)
                return;

            var assets = Resolve<IAssetManager>();
            var xmiBytes = assets.LoadSong(_songId);
            if ((xmiBytes?.Length ?? 0) == 0)
                return;

            _player.OpenBankData(assets.LoadSoundBanks());
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
