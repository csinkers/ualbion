using UAlbion.Api;
using UAlbion.Formats.Containers;

namespace UAlbion.Game.Assets;

public interface IContainerRegistry
{
    IAssetContainer GetContainer(string path, string container, IFileSystem disk);
}