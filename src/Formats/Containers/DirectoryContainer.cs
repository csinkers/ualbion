using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Containers;

/// <summary>
/// Sub-assets are just files in a directory, named 0_Foo, 1, 2_Bar etc (anything after an underscore is ignored when loading)
/// </summary>
public class DirectoryContainer : IAssetContainer
{
    public ISerializer Read(string path, AssetInfo info, SerdesContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var subAssets = new Dictionary<int, (string, string)>(); // path and name
        // Pattern vars: 0=Index 1=SubItem 2=Name 3=Palette
        var pattern = info.GetPattern(AssetProperty.Pattern, "{0}_{1}_{2}.dat");

        if (context.Disk.DirectoryExists(path))
        {
            foreach (var filePath in context.Disk.EnumerateDirectory(path, pattern.WilcardForIndex(info.Index)))
            {
                var filename = Path.GetFileName(filePath);
                if (!pattern.TryParse(filename, out var assetPath))
                    continue;

                if (assetPath.PaletteId.HasValue)
                    info.Set(AssetProperty.PaletteId, assetPath.PaletteId);

                if (assetPath.Index != info.Index)
                    continue;

                subAssets[assetPath.SubAsset] = (filePath, assetPath.Name);
            }
        }

        var ms = new MemoryStream();
        if (subAssets.Count > 0)
        {
            using var bw = new BinaryWriter(ms, Encoding.UTF8, true);
            using var s = new AlbionWriter(bw);
            PackedChunks.PackNamed(s, subAssets.Keys.Max() + 1, i => 
                !subAssets.TryGetValue(i, out var pathAndName)
                    ? (Array.Empty<byte>(), null)
                    : (context.Disk.ReadAllBytes(pathAndName.Item1), pathAndName.Item2));
        }

        ms.Position = 0;
        var br = new BinaryReader(ms);
        return new AlbionReader(br, ms.Length, () =>
        {
            br.Dispose();
            ms.Dispose();
        });
    }

    public void Write(string path, IList<(AssetInfo, byte[])> assets, SerdesContext context)
    {
        if (assets == null) throw new ArgumentNullException(nameof(assets));
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (context.Disk.FileExists(path))
            throw new InvalidOperationException($"Cannot save directory container at \"{path}\", as there is already a file with that name.");

        // Console.WriteLine($"Writing {string.Join(", ", assets.Select(x => $"{x.Item1.AssetId}:{x.Item2?.Length}"))} to {path}");

        if (!context.Disk.DirectoryExists(path))
            context.Disk.CreateDirectory(path);

        foreach (var (info, assetBytes) in assets)
        {
            if (assetBytes.Length == 0)
                continue;

            using var ms = new MemoryStream(assetBytes);
            using var br = new BinaryReader(ms);
            using var s = new AlbionReader(br);
            var subAssets = PackedChunks.Unpack(s).ToList();

            var pattern = info.GetPattern(AssetProperty.Pattern, "{0}_{1}_{2}.dat");

            if (subAssets.Count == 1)
            {
                var (subAssetBytes, name) = subAssets[0];
                var filename = name ?? pattern.Format(new AssetPath(info));
                context.Disk.WriteAllBytes(Path.Combine(path, filename), subAssetBytes);
            }
            else
            {
                for (int i = 0; i < subAssets.Count; i++)
                {
                    var (subAssetBytes, name) = subAssets[i];
                    if (subAssetBytes.Length == 0)
                        continue;

                    if (string.IsNullOrWhiteSpace(name))
                        name = null;

                    var filename = name ?? pattern.Format(new AssetPath(info, i));
                    var fullPath = Path.Combine(path, filename);

                    var dir = Path.GetDirectoryName(fullPath);
                    if (!context.Disk.DirectoryExists(dir))
                        context.Disk.CreateDirectory(dir);

                    context.Disk.WriteAllBytes(fullPath, subAssetBytes);
                }
            }
        }
    }

    public List<(int, int)> GetSubItemRanges(string path, AssetFileInfo info, SerdesContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        var subIds = new List<int>();
        if (!context.Disk.DirectoryExists(path))
            return new List<(int, int)> { (0, 1) };

        foreach (var filePath in context.Disk.EnumerateDirectory(path))
        {
            var file = Path.GetFileName(filePath);
            int index = file.IndexOf('_', StringComparison.Ordinal);
            var part = index == -1 ? file : file.Substring(0, index);
            if (!int.TryParse(part, out var asInt))
                continue;

            subIds.Add(asInt);
        }

        subIds.Sort();
        return FormatUtil.SortedIntsToRanges(subIds);
    }
}
