using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ADLMidi.NET;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Assets
{
    public class SoundBankLocator : IAssetLocator
    {
        public object LoadAsset(AssetKey key, string name, Func<AssetKey, string, object> loaderFunc)
        {
            if(key.Type != AssetType.SoundBank)
                throw new InvalidOperationException($"Called SoundBankLocator with unexpected asset type {key.Type}");

            var config = (IGeneralConfig)loaderFunc(new AssetKey(AssetType.GeneralConfig), "");
            var oplPath = Path.Combine(config.ExePath, "DRIVERS", "ALBISND.OPL");
            GlobalTimbreLibrary oplFile = ReadOpl(oplPath);
            WoplFile wopl = new WoplFile(oplFile);
            byte[] bankData = GetRawWoplBytes(wopl);
            return bankData;
        }

        public IEnumerable<AssetType> SupportedTypes => new[] { AssetType.SoundBank };

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
