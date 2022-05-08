using System.Collections.Generic;
using System.IO;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats;

namespace UAlbion.Game.Assets;

public interface IAssetLocator : IComponent
{
    object LoadAsset(AssetInfo info, SerdesContext context, TextWriter annotationWriter);
    List<(int,int)> GetSubItemRangesForFile(AssetFileInfo info, SerdesContext context);
}