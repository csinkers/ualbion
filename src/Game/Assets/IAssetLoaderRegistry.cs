using System;
using UAlbion.Config;

namespace UAlbion.Game.Assets;

public interface IAssetLoaderRegistry
{
    IAssetLoader GetLoader(Type loaderType);
}