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
        readonly IAssetLoaderRegistry _assetLoaderRegistry;
        readonly IContainerLoaderRegistry _containerLoaderRegistry;
        readonly MetaFontLoader _metafontLoader;

        public AssetLocator(IAssetLoaderRegistry assetLoaderRegistry, IContainerLoaderRegistry containerLoaderRegistry, MetaFontLoader metafontLoader)
        {
            _assetLoaderRegistry = assetLoaderRegistry ?? throw new ArgumentNullException(nameof(assetLoaderRegistry));
            _containerLoaderRegistry = containerLoaderRegistry ?? throw new ArgumentNullException(nameof(containerLoaderRegistry));
            _metafontLoader = metafontLoader ?? throw new ArgumentNullException(nameof(metafontLoader));
            AttachChild(_assetLoaderRegistry as IComponent);
            AttachChild(_containerLoaderRegistry as IComponent);
            AttachChild(_metafontLoader);
        }

        public object LoadAsset(AssetId id, SerializationContext context, AssetInfo info)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (id.Type == AssetType.MetaFont)
                return _metafontLoader.Load(id);

            if (info == null) throw new ArgumentNullException(nameof(info));
            var generalConfig = Resolve<IGeneralConfig>();
            object asset = TryLoad(context.ModDirectory, generalConfig, info, context);
            foreach (var dir in generalConfig.SearchPaths)
            {
                if (asset != null)
                    return asset;
                asset = TryLoad(dir, generalConfig, info, context);
            }

            if (asset == null)
                throw new FileNotFoundException($"Could not find file matching \"{info.Parent.Filename}\" in the configured mods and search paths (for asset {info.AssetId}).");

            return asset;
        }

        object TryLoad(string dir, IGeneralConfig generalConfig, AssetInfo info, SerializationContext context)
        {
            using var s = Search(dir, generalConfig, info);
            if (s == null)
                return null;

            var loader = _assetLoaderRegistry.GetLoader(info.Format);
            if (loader == null)
                throw new InvalidOperationException($"No loader for file type \"{info.Format}\" required by asset {info.Name}");

            return loader.Serdes(null, info, context.Mapping, s);
        }

        ISerializer Search(string dir, IGeneralConfig generalConfig, AssetInfo info)
        {
            var combined = Path.Combine(dir, info.Parent.Filename);
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
                if (!string.IsNullOrEmpty(info.Parent.Sha256Hash))
                {
                    var hash = GetHash(path);
                    if (!hash.Equals(info.Parent.Sha256Hash, StringComparison.OrdinalIgnoreCase))
                    {
                        CoreUtil.LogWarn($"Found file {path} for asset {info.AssetId}, but its hash ({hash}) did not match the expected one ({info.Parent.Sha256Hash})");
                        return null;
                    }
                }

                ISerializer s;
                var extension = Path.GetExtension(path).ToUpperInvariant();
                switch (extension)
                {
                    case ".XLD": s = _containerLoaderRegistry.Load(path, info, ContainerFormat.Xld); break;
                    case ".ZIP": s = _containerLoaderRegistry.Load(path, info, ContainerFormat.Zip); break;
                    default: s = _containerLoaderRegistry.Load(path, info, info.Parent.ContainerFormat); break;
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
