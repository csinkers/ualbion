using System.Collections.Generic;
using System.IO;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats;

namespace UAlbion.Game.Assets;

public interface IAssetLocator : IComponent
{
    object LoadAsset(AssetInfo info, LoaderContext context, IDictionary<string, string> extraPaths, TextWriter annotationWriter);
    List<(int,int)> GetSubItemRangesForFile(AssetFileInfo info, IDictionary<string, string> extraPaths);
}