using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Core;

namespace UAlbion.Game.Assets
{
    public interface IAssetLocator : IComponent
    {
        object LoadAsset(AssetId id, AssetMapping mapping, AssetInfo info, IDictionary<string, string> extraPaths);
        List<(int,int)> GetSubItemRangesForFile(AssetFileInfo info, IDictionary<string, string> extraPaths);
    }
}
