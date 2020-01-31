using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    public static class CoreSpriteLoader
    {
        static string GetHash(string filename)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filename);
            var hashBytes = md5.ComputeHash(stream);
            return string.Join("", hashBytes.Select(x => x.ToString("x2")));
        }

        static byte[] LoadSection(string filename, CoreSpriteConfig.BinaryResource resource)
        {
            using (var stream = File.OpenRead(filename))
            using (var br = new BinaryReader(stream))
            {
                stream.Position = resource.Offset;
                return br.ReadBytes(resource.Width * resource.Height);
            }
        }

        public static CoreSpriteConfig.BinaryResource GetConfig(CoreSpriteId id, string exePath, CoreSpriteConfig config, out string filename)
        {
            if(!Directory.Exists(exePath))
                throw new InvalidOperationException($"Search directory {exePath} does not exist");

            foreach (var file in Directory.EnumerateFiles(exePath, "*.*", SearchOption.AllDirectories))
            {
                if(!file.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                var hash = GetHash(file);
                if (config.Hashes.TryGetValue(hash, out var resources))
                {
                    filename = file;
                    return resources[(int)id];
                }
            }

            throw new FileNotFoundException("No suitable main.exe file could be found.");
        }

        public static AlbionSprite Load(CoreSpriteId id, string exePath, CoreSpriteConfig config)
        {
            var resource = GetConfig(id, exePath, config, out var file);
            var bytes = LoadSection(file, resource);
            return new AlbionSprite
            {
                Name = $"Core.{id}",
                Width = resource.Width,
                Height = resource.Height,
                UniformFrames = true,
                Frames = new[] { new AlbionSprite.Frame(0, 0, resource.Width, resource.Height) },
                PixelData = bytes
            };
        }
    }
}
