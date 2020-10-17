using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ADLMidi.NET;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Assets
{
    public class SoundBankLocator : IAssetLocator
    {
        public object LoadAsset(AssetId key, SerializationContext context, Func<AssetId, SerializationContext, object> loaderFunc)
        {
            if (loaderFunc == null) throw new ArgumentNullException(nameof(loaderFunc));
            if (key != AssetId.SoundBank)
                throw new InvalidOperationException($"Called SoundBankLocator with unexpected asset type {key.Type}");

            var config = (IGeneralConfig)loaderFunc(AssetId.GeneralConfig, context);
            var oplPath = Path.Combine(config.BasePath, config.ExePath, "DRIVERS", "ALBISND.OPL");
            GlobalTimbreLibrary oplFile = ReadOpl(oplPath);
            WoplFile wopl = new WoplFile(oplFile);
            byte[] bankData = GetRawWoplBytes(wopl);
            return bankData;
        }

        public IEnumerable<AssetType> SupportedTypes => new[] { AssetType.Special };

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

        public AssetInfo GetAssetInfo(AssetId key, Func<AssetId, SerializationContext, object> loaderFunc) => null;
    }
}
