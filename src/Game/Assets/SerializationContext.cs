using System;
using UAlbion.Config;
using UAlbion.Formats;

namespace UAlbion.Game.Assets
{
    public class SerializationContext
    {
        public SerializationContext(AssetMapping mapping, GameLanguage language)
        {
            Mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
            Language = language;
        }

        public AssetMapping Mapping { get; }
        public GameLanguage Language { get; }
    }
}