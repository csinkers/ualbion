using System.IO;
using System.Text;
using ADLMidi.NET;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;

namespace UAlbion.Game.Assets
{
    public class SoundBankLoader : Component, IAssetLoader
    {
        static byte[] GetRawWoplBytes(WoplFile wopl)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            using var gbw = new GenericBinaryWriter(bw, Encoding.ASCII.GetBytes, ApiUtil.Assert);
            WoplFile.Serdes(wopl, gbw);
            return ms.ToArray();
        }

        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            var oplFile = GlobalTimbreLibrary.Serdes(null, s);
            WoplFile wopl = new WoplFile(oplFile);
            return GetRawWoplBytes(wopl);
        }
    }
}
