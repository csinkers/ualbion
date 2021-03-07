using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Containers
{
    /// <summary>
    /// Sub-assets are just files in a directory, named 0_Foo, 1, 2_Bar etc (anything after an underscore is ignored when loading)
    /// </summary>
    public class DirectoryContainer : IAssetContainer
    {
        public ISerializer Read(string path, AssetInfo info, IFileSystem disk)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            foreach (var filePath in disk.EnumerateDirectory(path, $"{info.SubAssetId}*.*"))
            {
                var file = Path.GetFileName(filePath);
                int index = file.IndexOf('_');
                var prefix = index == -1 ? file : file.Substring(0, index);
                if (!int.TryParse(prefix, out int numeric))
                    continue;

                if (numeric != info.SubAssetId)
                    continue;

                var stream = disk.OpenRead(filePath);
                var br = new BinaryReader(stream);
                return new AlbionReader(br, stream.Length, () => { br.Dispose(); stream.Dispose(); });
            }

            return null;
        }

        public void Write(string path, IList<(AssetInfo, byte[])> assets, IFileSystem disk)
        {
            if (assets == null) throw new ArgumentNullException(nameof(assets));
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            if (disk.FileExists(path))
                throw new InvalidOperationException($"Cannot save directory container at \"{path}\", as there is already a file with that name.");

            if (!disk.DirectoryExists(path))
                disk.CreateDirectory(path);

            foreach (var (info, bytes) in assets)
            {
                if (bytes.Length == 0)
                    continue;

                using var ms = new MemoryStream(bytes);
                using var br = new BinaryReader(ms);
                using var s = new AlbionReader(br);
                var frames = PackedChunks.Unpack(s).ToList();

                var pattern = info.Get(AssetProperty.Pattern, "{0}_{1}_{2}.dat");
                var name = ConfigUtil.AssetName(info.AssetId);
                if (frames.Count == 1)
                {
                    var filename = string.Format(CultureInfo.InvariantCulture, pattern, info.SubAssetId, 0, name);
                    disk.WriteAllBytes(Path.Combine(path, filename), frames[0]);
                }
                else
                {
                    for (int i = 0; i < frames.Count; i++)
                    {
                        if (frames[i].Length == 0)
                            continue;

                        var filename = string.Format(CultureInfo.InvariantCulture, pattern, info.SubAssetId, i, name);
                        disk.WriteAllBytes(Path.Combine(path, filename), frames[i]);
                    }
                }
            }
        }

        public List<(int, int)> GetSubItemRanges(string path, AssetFileInfo info, IFileSystem disk)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            var subIds = new List<int>();
            if (!disk.DirectoryExists(path))
                return new List<(int, int)> { (0, 1) };

            foreach (var filePath in disk.EnumerateDirectory(path))
            {
                var file = Path.GetFileName(filePath);
                int index = file.IndexOf('_');
                var part = index == -1 ? file : file.Substring(0, index);
                if (!int.TryParse(part, out var asInt))
                    continue;

                subIds.Add(asInt);
            }

            subIds.Sort();
            return FormatUtil.SortedIntsToRanges(subIds);
        }
    }
}