using System.Collections.Generic;
using SerdesNet;

namespace UAlbion.Config;

/// <summary>
/// Abstraction over container files/directories containing assets
/// In general, the assets inside containers are identified by a numeric sub-id,
/// and then mapped to AssetIds via the assets.json config file.
/// </summary>
public interface IAssetContainer
{
    /// <summary>
    /// Open a serializer for one of the assets inside a container
    /// </summary>
    /// <param name="path">The path to the container file/directory</param>
    /// <param name="context">The loader context, containing the JSON serialiser, file system access object etc</param>
    /// <returns></returns>
    ISerdes Read(string path, AssetLoadContext context);

    /// <summary>
    /// Write all assets inside a container
    /// </summary>
    /// <param name="path">The path to the container file/directory</param>
    /// <param name="assets">A list of pairs containing asset metadata and the corresponding raw bytes of the asset</param>
    /// <param name="context">The loader context, containing the JSON serialiser, file system access object etc</param>
    void Write(string path, IList<(AssetLoadContext, byte[])> assets, ModContext context);
}