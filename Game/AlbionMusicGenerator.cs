using System;
using System.IO;
using System.Reflection;
using System.Text;
using ADLMidi.NET;
using SerdesNet;
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

        public override void Subscribed()
        {
            if (!File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "ADLMIDI.dll")))
                return;

            if (_player != null)
                return;

            var assets = Resolve<IAssetManager>();
            var config = assets.LoadGeneralConfig();
            var xmiBytes = assets.LoadSong(_songId);
            if ((xmiBytes?.Length ?? 0) == 0)
                return;

            _player = AdlMidi.Init();

            var oplPath = Path.Combine(config.ExePath, "DRIVERS", "ALBISND.OPL");
            GlobalTimbreLibrary oplFile = ReadOpl(oplPath);
            WoplFile wopl = new WoplFile(oplFile);
            byte[] bankData = GetRawWoplBytes(wopl);

            _player.OpenBankData(bankData);

            _player.OpenData(xmiBytes);
            _player.SetLoopEnabled(true);
        }

        public override void Detach()
        {
            _player?.Close();
            _player = null;
            base.Detach();
        }

        public int FillBuffer(Span<short> buffer) => _player?.Play(buffer) ?? 0;

        static GlobalTimbreLibrary ReadOpl(string filename)
        {
            using var stream = File.OpenRead(filename);
            using var br = new BinaryReader(stream);
            return GlobalTimbreLibrary.Serdes(null,
                new GenericBinaryReader(br, br.BaseStream.Length, Encoding.ASCII.GetString, ApiUtil.Assert));
        }

        static byte[] GetRawWoplBytes(WoplFile wopl)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            WoplFile.Serdes(wopl, new GenericBinaryWriter(bw, Encoding.ASCII.GetBytes, ApiUtil.Assert));
            return ms.ToArray();
        }
    }
}
