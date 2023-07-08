using System.Collections.Generic;
using System.IO;
using UAlbion.Api.Eventing;
using UAlbion.Config;

namespace UAlbion.Game.Assets;

public interface IAssetLocator : IComponent
{
    object LoadAsset(AssetLoadContext context, TextWriter annotationWriter, List<string> filesSearched);
}