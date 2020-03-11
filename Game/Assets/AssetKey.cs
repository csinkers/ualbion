using UAlbion.Formats;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Assets
{
    public class AssetKey
    {
        public AssetKey(AssetType type, int id = 0, GameLanguage language = GameLanguage.English)
        {
            Type = type;
            Id = id;
            Language = language;
        }

        public AssetType Type { get; }
        public int Id { get; }
        public GameLanguage Language { get; }
    }
}
