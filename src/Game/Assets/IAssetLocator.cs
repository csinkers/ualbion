using System.Collections.Generic;
using System.IO;
using UAlbion.Config;
using UAlbion.Core;

namespace UAlbion.Game.Assets;

public interface IAssetLocator : IComponent
{
    object LoadAsset(AssetInfo info, AssetMapping mapping, IDictionary<string, string> extraPaths, TextWriter annotationWriter);
    List<(int,int)> GetSubItemRangesForFile(AssetFileInfo info, IDictionary<string, string> extraPaths);
}