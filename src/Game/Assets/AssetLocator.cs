using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Core;
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
            return TryLoad(generalConfig, info, mapping, extraPaths);
        }

        public List<(int,int)> GetSubItemRangesForFile(AssetFileInfo info, IDictionary<string, string> extraPaths)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            var generalConfig = Resolve<IGeneralConfig>();
            var resolved = generalConfig.ResolvePath(info.Filename, extraPaths);
            var containerLoader = GetContainerLoader(resolved, info.ContainerFormat);
            return containerLoader?.GetSubItemRanges(resolved, info) ?? new List<(int, int)> { (0, 1) };
        }

        object TryLoad(IGeneralConfig generalConfig, AssetInfo info, AssetMapping mapping, IDictionary<string, string> extraPaths)
        {
            using var s = Search(generalConfig, info, extraPaths);
            if (s == null)
                return null;

            if (s.BytesRemaining == 0) // Happens all the time when dumping, just return rather than throw to preserve perf.
                return new AssetNotFoundException($"Asset for {info.AssetId} found but size was 0 bytes.", info.AssetId);

            var loader = _assetLoaderRegistry.GetLoader(info.File.Loader);
            if (loader == null)
                throw new InvalidOperationException($"No loader for file type \"{info.File.Loader}\" required by asset {info.AssetId}");

            return loader.Serdes(null, info, mapping, s);
        }

        IContainerLoader GetContainerLoader(string path, ContainerFormat format)
        {
            if (File.Exists(path))
            {
                if (format == ContainerFormat.Auto)
                {
                    format = Path.GetExtension(path).ToUpperInvariant() switch
                    {
                        ".XLD" => ContainerFormat.Xld,
                        ".ZIP" => ContainerFormat.Zip,
                        _ => throw new InvalidOperationException(
                            $"Could not autodetect container type for file \"{path}\"")
                    };
                }
            }
            else if (Directory.Exists(path))
            {
                format = ContainerFormat.Directory;
            }
            else
            {
                return null; // Not found
            }

            return _containerLoaderRegistry.GetLoader(format);
        }

        ISerializer Search(IGeneralConfig generalConfig, AssetInfo info, IDictionary<string, string> extraPaths)
        {
            var path = generalConfig.ResolvePath(info.File.Filename, extraPaths);
            if (info.File.Sha256Hash != null && !info.File.Sha256Hash.Equals(GetHash(path), StringComparison.OrdinalIgnoreCase))
                return null;

            var containerLoader = GetContainerLoader(path, info.File.ContainerFormat);
            return containerLoader?.Open(path, info);
        }

        string GetHash(string filename)
        {
            if (_hashCache.TryGetValue(filename, out var hash))
                return hash;

            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filename);
            var hashBytes = sha256.ComputeHash(stream);
            hash = string.Join("", hashBytes.Select(x => x.ToString("x2", CultureInfo.InvariantCulture)));

            _hashCache[filename] = hash;
            return hash;
        }
    }
}
