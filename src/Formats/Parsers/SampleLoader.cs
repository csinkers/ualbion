using System;
using System.IO;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.AudioSample, FileFormat.SampleLibrary)]
    public class SampleLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, AssetMapping mapping, AssetId id, AssetInfo config)
        {
            if (br == null) throw new ArgumentNullException(nameof(br));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (config.Format == FileFormat.AudioSample)
                return new AlbionSample(br.ReadBytes((int) streamLength));

            if (config.Format == FileFormat.SampleLibrary)
                return WaveLib.Serdes(null, new GenericBinaryReader(br, streamLength, FormatUtil.BytesTo850String, ApiUtil.Assert));

            throw new InvalidOperationException($"Tried to load asset of invalid type \"{config.Format}\" in SampleLoader");
        }
    }
}
