using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.AudioSample, FileFormat.SampleLibrary)]
    public class SampleLoader : IAssetLoader
    {
        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (config.Format == FileFormat.AudioSample)
                return new AlbionSample(s.ByteArray(null, null, (int)s.BytesRemaining));

            if (config.Format != FileFormat.SampleLibrary)
                throw new InvalidOperationException($"Tried to load asset of invalid type \"{config.Format}\" in SampleLoader");

            return WaveLib.Serdes(null, s);
        }
    }
}
