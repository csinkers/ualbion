using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using SerdesNet;

namespace UAlbion.Formats.Containers
{
    public class BinaryOffsetContainer : IFileContainer
    {
        public ISerializer Open(string file, string subItem)
        {
            throw new NotImplementedException();
        }

        static string GetHash(string filename)
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filename);
            var hashBytes = sha256.ComputeHash(stream);
            return string.Join("", hashBytes.Select(x => x.ToString("x2", CultureInfo.InvariantCulture)));
        }
/*
        static byte[] LoadSection(string filename, CoreSpriteInfo resource)
        {
            using var stream = File.OpenRead(filename);
            using var br = new BinaryReader(stream);
            stream.Position = resource.Offset;
            return br.ReadBytes(resource.Width * resource.Height);
        }

        public static CoreSpriteInfo GetConfig(SpriteId id, string exePath, CoreSpriteConfig config, out string filename)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (!Directory.Exists(exePath))
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

        public static AlbionSprite Load(SpriteId id, string exePath, CoreSpriteConfig config)
        {
            var resource = GetConfig(id, exePath, config, out var file);
            var bytes = LoadSection(file, resource);
            return new AlbionSprite(
                $"Core.{id}",
                resource.Width,
                resource.Height,
                true,
                bytes,
                new[] { new AlbionSpriteFrame(0, 0, resource.Width, resource.Height) });
        }
*/
    }
}
