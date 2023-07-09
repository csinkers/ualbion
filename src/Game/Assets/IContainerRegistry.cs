using System;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Game.Assets;

public interface IContainerRegistry
{
    IAssetContainer GetContainer(string path, Type container, IFileSystem disk);
}