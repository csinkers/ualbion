using System;
using UAlbion.Config;
using UAlbion.Formats;

namespace UAlbion.Game.Assets
{
    public class SerializationContext
    {
        public SerializationContext(AssetMapping mapping, GameLanguage language, string modDirectory)
        {
            Mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
            Language = language;
            ModDirectory = modDirectory;
        }

        public AssetMapping Mapping { get; }
        public GameLanguage Language { get; }
        public string ModDirectory { get; }
    }
}