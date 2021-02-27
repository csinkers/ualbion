using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Containers;

namespace UAlbion.Game.Assets
{
    public sealed class AssetLocator : ServiceComponent<IAssetLocator>, IAssetLocator
    {
        readonly IDictionary<string, string> _hashCache = new Dictionary<string, string>();
        IAssetLoaderRegistry _assetLoaderRegistry;
        IContainerLoaderRegistry _containerLoaderRegistry;

        protected override void Subscribed()
        {
            _assetLoaderRegistry = Resolve<IAssetLoaderRegistry>();
            _containerLoaderRegistry = Resolve<IContainerLoaderRegistry>();
            base.Subscribed();
        }

        public object LoadAsset(AssetId id, AssetMapping mapping, AssetInfo info, IDictionary<string, string> extraPaths)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (extraPaths == null) throw new ArgumentNullException(nameof(extraPaths));
            var generalConfig = Resolve<IGeneralConfig>();

            using ISerializer s = Search(generalConfig, info, extraPaths);
            if (s == null)
                return null;

            if (s.BytesRemaining == 0) // Happens all the time when dumping, just return rather than throw to preserve perf.
                return new AssetNotFoundException($"Asset for {info.AssetId} found but size was 0 bytes.", info.AssetId);

            var loader = _assetLoaderRegistry.GetLoader(info.File.Loader);
            if (loader == null)
                throw new InvalidOperationException($"Could not instantiate loader \"{info.File.Loader}\" required by asset {info.AssetId}");

            return loader.Serdes(null, info, mapping, s);
        }

        public List<(int,int)> GetSubItemRangesForFile(AssetFileInfo info, IDictionary<string, string> extraPaths)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            var generalConfig = Resolve<IGeneralConfig>();
            var resolved = generalConfig.ResolvePath(info.Filename, extraPaths);
            var containerLoader = GetContainerLoader(resolved, info.Container);
            return containerLoader?.GetSubItemRanges(resolved, info) ?? new List<(int, int)> { (0, 1) };
        }

        IContainerLoader GetContainerLoader(string path, string container)
        {
            if (!string.IsNullOrEmpty(container))
                return _containerLoaderRegistry.GetLoader(container);

            if (File.Exists(path))
            {
                return Path.GetExtension(path).ToUpperInvariant() switch
                {
                    ".XLD" => _containerLoaderRegistry.GetLoader(typeof(XldContainerLoader)),
                    ".ZIP" => _containerLoaderRegistry.GetLoader(typeof(ZipContainer)),
                    _ => throw new InvalidOperationException($"Could not autodetect container type for file \"{path}\"")
                };
            }

            return Directory.Exists(path) 
                ? _containerLoaderRegistry.GetLoader(typeof(DirectoryContainerLoader)) 
                : null;
        }

        ISerializer Search(IGeneralConfig generalConfig, AssetInfo info, IDictionary<string, string> extraPaths)
        {
            var path = generalConfig.ResolvePath(info.File.Filename, extraPaths);
            if (info.File.Sha256Hash != null && !info.File.Sha256Hash.Equals(GetHash(path), StringComparison.OrdinalIgnoreCase))
                return null;

            var containerLoader = GetContainerLoader(path, info.File.Container);
            return containerLoader?.Open(path, info);
        }

        string GetHash(string filename)
        {
            if (_hashCache.TryGetValue(filename, out var hash))
                return hash;

            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filename);
            var hashBytes = sha256.ComputeHash(stream);
            hash = FormatUtil.BytesToHexString(hashBytes);

            _hashCache[filename] = hash;
            return hash;
        }
    }
}
