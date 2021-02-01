using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Core;

namespace UAlbion.Game.Assets
{
    public sealed class AssetLocator : ServiceComponent<IAssetLocator>, IAssetLocator
    {
        IAssetLoaderRegistry _assetLoaderRegistry;
        IContainerLoaderRegistry _containerLoaderRegistry;

        protected override void Subscribed()
        {
            _assetLoaderRegistry = Resolve<IAssetLoaderRegistry>();
            _containerLoaderRegistry = Resolve<IContainerLoaderRegistry>();
            base.Subscribed();
        }

        public object LoadAsset(AssetId id, SerializationContext context, AssetInfo info)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (info == null) throw new ArgumentNullException(nameof(info));
            var generalConfig = Resolve<IGeneralConfig>();
            object asset = TryLoad(context.ModAssetDirectory, generalConfig, info, context);
            foreach (var dir in generalConfig.SearchPaths)
            {
                if (asset != null)
                    return asset;
                asset = TryLoad(dir, generalConfig, info, context);
            }

            if (asset == null)
                throw new AssetNotFoundException($"Could not find file matching \"{info.File.Filename}\" in the configured mods and search paths (for asset {info.AssetId}).", info.AssetId);

            return asset;
        }

        object TryLoad(string dir, IGeneralConfig generalConfig, AssetInfo info, SerializationContext context)
        {
            using var s = Search(dir, generalConfig, info);
            if (s == null)
                return null;

            if (s.BytesRemaining == 0) // Happens all the time when dumping, just return rather than throw to preserve perf.
                return new AssetNotFoundException($"Asset for {info.AssetId} found but size was 0 bytes.", info.AssetId);

            var loader = _assetLoaderRegistry.GetLoader(info.File.Loader);
            if (loader == null)
                throw new InvalidOperationException($"No loader for file type \"{info.File.Loader}\" required by asset {info.Name}");

            return loader.Serdes(null, info, context.Mapping, s);
        }

        ISerializer Search(string dir, IGeneralConfig generalConfig, AssetInfo info)
        {
            var combined = Path.Combine(dir, info.File.Filename);
            var filename = Path.GetFileName(combined).ToUpperInvariant();
            dir = Path.GetDirectoryName(combined);
            var resolved = generalConfig.ResolvePath(dir);
            if (!Directory.Exists(resolved))
                return null;

            var directory = Path.Combine(resolved, filename);
            if (Directory.Exists(directory))
            {
                var s = _containerLoaderRegistry.Load(directory, info, ContainerFormat.Directory);
                if (s != null)
                    return s;
            }

            var files = Directory.GetFiles(resolved);
            foreach (var path in files.Where(x => Path.GetFileNameWithoutExtension(x).ToUpperInvariant() == filename))
            {
                if (info.File.Sha256Hashes != null)
                {
                    var hash = GetHash(path);
                    if (info.File.Sha256Hashes.All(x => !hash.Equals(x, StringComparison.OrdinalIgnoreCase)))
                    {
                        var expected = string.Join(", ", info.File.Sha256Hashes);
                        CoreUtil.LogWarn(
                            $"Found file {path} for asset {info.AssetId}, but its " + 
                            $"hash ({hash}) did not match any of the expected ones ({expected})");
                        return null;
                    }
                }

                ISerializer s;
                var extension = Path.GetExtension(path).ToUpperInvariant();
                switch (extension)
                {
                    case ".XLD": s = _containerLoaderRegistry.Load(path, info, ContainerFormat.Xld); break;
                    case ".ZIP": s = _containerLoaderRegistry.Load(path, info, ContainerFormat.Zip); break;
                    default: s = _containerLoaderRegistry.Load(path, info, info.File.ContainerFormat); break;
                }

                if (s != null)
                    return s;
            }

            return null;
        }
        static string GetHash(string filename)
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filename);
            var hashBytes = sha256.ComputeHash(stream);
            return string.Join("", hashBytes.Select(x => x.ToString("x2", CultureInfo.InvariantCulture)));
        }
    }
}
