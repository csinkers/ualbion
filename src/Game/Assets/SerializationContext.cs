using System;
using UAlbion.Config;

namespace UAlbion.Game.Assets
{
    public class SerializationContext
    {
        public SerializationContext(AssetMapping mapping, string modAssetDirectory)
        {
            Mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
            ModAssetDirectory = modAssetDirectory;
        }

        public AssetMapping Mapping { get; }
        public string ModAssetDirectory { get; }
    }
}