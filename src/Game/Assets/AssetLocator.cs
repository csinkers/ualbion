using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats;

namespace UAlbion.Game.Assets;

public sealed class AssetLocator : ServiceComponent<IAssetLocator>, IAssetLocator
{
    readonly IDictionary<string, string> _hashCache = new Dictionary<string, string>();
    IAssetLoaderRegistry _assetLoaderRegistry;
    IContainerRegistry _containerRegistry;

    protected override void Subscribed()
    {
        _assetLoaderRegistry = Resolve<IAssetLoaderRegistry>();
        _containerRegistry = Resolve<IContainerRegistry>();
        base.Subscribed();
    }

    public object LoadAsset(AssetInfo info, SerdesContext context, TextWriter annotationWriter = null)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (context == null) throw new ArgumentNullException(nameof(context));
        var pathResolver = Resolve<IPathResolver>();

        using ISerializer s = Search(pathResolver, info, context, annotationWriter);
        if (s == null)
            return null;

        if (s.BytesRemaining == 0 && s is not EmptySerializer) // Happens all the time when dumping, just return rather than throw to preserve perf.
            return new AssetNotFoundException($"Asset for {info.AssetId} found but size was 0 bytes.", info.AssetId);

        var loader = _assetLoaderRegistry.GetLoader(info.File.Loader);
        if (loader == null)
            throw new InvalidOperationException($"Could not instantiate loader \"{info.File.Loader}\" required by asset {info.AssetId}");

        return loader.Serdes(null, info, s, context);
    }

    public List<(int,int)> GetSubItemRangesForFile(AssetFileInfo info, SerdesContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var pathResolver = Resolve<IPathResolver>();
        var resolved = pathResolver.ResolvePath(info.Filename);
        var container = _containerRegistry.GetContainer(resolved, info.Container, context.Disk);
        return container?.GetSubItemRanges(resolved, info, context) ?? new List<(int, int)> { (0, 1) };
    }

    ISerializer Search(IPathResolver pathResolver, AssetInfo info, SerdesContext context, TextWriter annotationWriter = null)
    {
        var path = pathResolver.ResolvePath(info.File.Filename);
        if (info.File.Sha256Hash != null && !info.File.Sha256Hash.Equals(GetHash(path, context.Disk), StringComparison.OrdinalIgnoreCase))
            return null;

        var container = _containerRegistry.GetContainer(path, info.File.Container, context.Disk);
        var s = container?.Read(path, info, context);
        if (annotationWriter != null)
            s = new AnnotationProxySerializer(s, annotationWriter, FormatUtil.BytesFrom850String);
        return s;
    }

    string GetHash(string filename, IFileSystem disk)
    {
        if (_hashCache.TryGetValue(filename, out var hash))
            return hash;

        using var sha256 = SHA256.Create();
        using var stream = disk.OpenRead(filename);
        var hashBytes = sha256.ComputeHash(stream);
        hash = FormatUtil.BytesToHexString(hashBytes);

        _hashCache[filename] = hash;
        return hash;
    }
}
