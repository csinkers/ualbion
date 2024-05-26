using System;
using System.Collections.Generic;
using System.IO;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats;

namespace UAlbion.Game.Assets;

public sealed class AssetLocator : ServiceComponent<IAssetLocator>, IAssetLocator
{
    readonly Dictionary<string, string> _hashCache = new();
    IAssetLoaderRegistry _assetLoaderRegistry;
    IContainerRegistry _containerRegistry;

    protected override void Subscribed()
    {
        _assetLoaderRegistry = Resolve<IAssetLoaderRegistry>();
        _containerRegistry = Resolve<IContainerRegistry>();
        base.Subscribed();
    }

    public object LoadAsset(AssetLoadContext context, TextWriter annotationWriter, List<string> filesSearched)
    {
        ArgumentNullException.ThrowIfNull(context);
        var pathResolver = Resolve<IPathResolver>();

        if (context.Node.Filename == null || context.Node.Filename.StartsWith('!')) // If this is a meta-asset (e.g. is loaded from another asset)
        {
            var metaLoader = _assetLoaderRegistry.GetLoader(context.Node.Loader);
            if (metaLoader == null)
                throw new InvalidOperationException($"Could not instantiate loader \"{context.Node.Loader}\" required by asset {context.AssetId}");

            return metaLoader.Serdes(null, null, context);
        }

        using ISerializer s = Search(pathResolver, context, annotationWriter, filesSearched);
        if (s == null)
            return null;

        if (s.BytesRemaining == 0 && s is not EmptySerializer) // Happens all the time when dumping, just return rather than throw to preserve perf.
            return new AssetNotFoundException($"Asset for {context.AssetId} found but size was 0 bytes.", context.AssetId);

        var loader = _assetLoaderRegistry.GetLoader(context.Node.Loader);
        if (loader == null)
            throw new InvalidOperationException($"Could not instantiate loader \"{context.Node.Loader}\" required by asset {context.AssetId}");

        return loader.Serdes(null, s, context);
    }

    ISerializer Search(IPathResolver pathResolver, AssetLoadContext context, TextWriter annotationWriter, List<string> filesSearched)
    {
        var node = context.Node;
        var disk = context.ModContext.Disk;
        var path = pathResolver.ResolvePath(node.Filename);

        if (disk.FileExists(path) || disk.DirectoryExists(path))
        {
            if (node.Sha256Hash != null)
            {
                var hash = GetHash(path, disk);
                filesSearched?.Add($"{disk.ToAbsolutePath(path)} (actual hash {hash})");
                if (!node.Sha256Hash.Equals(hash, StringComparison.OrdinalIgnoreCase))
                    return null;
            }
            else filesSearched?.Add(disk.ToAbsolutePath(path));
        }

        var container = _containerRegistry.GetContainer(path, node.Container, disk);
        var s = container?.Read(path, context);
        if (annotationWriter != null)
            s = new AnnotationProxySerializer(s, annotationWriter, FormatUtil.BytesFrom850String);
        return s;
    }

    string GetHash(string filename, IFileSystem disk)
    {
        if (_hashCache.TryGetValue(filename, out var hash))
            return hash;

        hash = FormatUtil.GetReducedSha256HexString(filename, disk);

        _hashCache[filename] = hash;
        return hash;
    }
}
