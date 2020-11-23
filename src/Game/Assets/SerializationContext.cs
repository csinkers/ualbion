using System;
using UAlbion.Config;

namespace UAlbion.Game.Assets
{
    public class SerializationContext
    {
        public SerializationContext(AssetMapping mapping, string modDirectory)
        {
            Mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
            ModDirectory = modDirectory;
        }

        public AssetMapping Mapping { get; }
        public string ModDirectory { get; }
    }
}