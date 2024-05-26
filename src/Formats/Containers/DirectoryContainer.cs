using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Config.Properties;
using UAlbion.Formats.Ids;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Containers;

/// <summary>
/// Sub-assets are just files in a directory, named 0_Foo, 1, 2_Bar etc (anything after an underscore is ignored when loading)
/// </summary>
public class DirectoryContainer : IAssetContainer
{
    static readonly AssetPathPattern DefaultPattern = AssetPathPattern.Build("{0}_{1}_{2}.dat");
    public ISerializer Read(string path, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var subAssets = new Dictionary<int, (string, string)>(); // path and name
        // Pattern vars: 0=Index 1=SubItem 2=Name 3=Palette
        var pattern = context.GetProperty(AssetProps.Pattern, DefaultPattern);

        if (context.Disk.DirectoryExists(path))
        {
            var wildcardPattern = pattern.WilcardForId(context.AssetId);
            foreach (var filePath in context.Disk.EnumerateFiles(path, wildcardPattern))
            {
                var filename = Path.GetFileName(filePath);
                if (!pattern.TryParse(filename, context.AssetId.Type, out var assetPath))
                    continue;

                if (assetPath.PaletteId.HasValue)
                {
                    var palId = new PaletteId(assetPath.PaletteId.Value);
                    context.SetProperty(AssetProps.Palette, palId);
                }

                if (assetPath.AssetId != context.AssetId)
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

    public void Write(string path, IList<(AssetLoadContext, byte[])> assets, ModContext context)
    {
        ArgumentNullException.ThrowIfNull(assets);
        ArgumentNullException.ThrowIfNull(context);

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

            var pattern = info.GetProperty(AssetProps.Pattern, AssetPathPattern.Build("{0}_{1}_{2}.dat"));

            if (subAssets.Count == 1)
            {
                var (subAssetBytes, name) = subAssets[0];
                var filename = name ?? pattern.Format(info.BuildAssetPath());
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

                    var filename = name ?? pattern.Format(info.BuildAssetPath(i));
                    var fullPath = Path.Combine(path, filename);

                    var dir = Path.GetDirectoryName(fullPath);
                    if (!context.Disk.DirectoryExists(dir))
                        context.Disk.CreateDirectory(dir);

                    context.Disk.WriteAllBytes(fullPath, subAssetBytes);
                }
            }
        }
    }
}
