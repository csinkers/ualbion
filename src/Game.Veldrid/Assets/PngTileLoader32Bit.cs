using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SerdesNet;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Veldrid;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Maps;
using Veldrid;

namespace UAlbion.Game.Veldrid.Assets;

public class PngTileLoader32Bit : Component, IAssetLoader<ITileGraphics>
{
    const PixelFormat TextureFormat = PixelFormat.R8_G8_B8_A8_UNorm;
    record FrameInfo(string Path, int SubId, int PalFrame);
    readonly PngDecoder _decoder = new();
    readonly Configuration _configuration = new();

    public object Serdes(object existing, AssetInfo info, ISerializer s, SerdesContext context)
        => Serdes((ITileGraphics)existing, info, s, context);

    public ITileGraphics Serdes(ITileGraphics existing, AssetInfo info, ISerializer s, SerdesContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (s.IsWriting())
            throw new NotSupportedException("Saving png tile graphics is not currently supported");

        return Load(info, context);
    }

    ITileGraphics Load(AssetInfo info, SerdesContext context)
    {
        var engine = (Engine)Resolve<IEngine>();
        var (dayInfo, nightInfo) = FindFiles(info, context.Disk);

        var totalPngs =
            1 + // Region 0 = Blank instance
            dayInfo.Sum(x => x.Paths.Length) +
            (nightInfo?.Sum(x => x.Paths.Length) ?? 0);

        var (tileWidth, tileHeight) = GetPngSize(dayInfo.First().Paths[0], context.Disk);

        var limits = engine.GetPixelFormatProperties(TextureFormat);
        if (limits == null)
            throw new InvalidOperationException($"Graphics backend does not support pixel format \"{TextureFormat}\"");

        var layout = SpriteSheetUtil.ArrangeUniform(tileWidth, tileHeight,
            (int)limits.Value.MaxWidth,
            (int)limits.Value.MaxHeight,
            (int)limits.Value.MaxArrayLayers,
            totalPngs);

        var texture = new ArrayTexture<uint>(info.AssetId, info.ToString(), layout.Width, layout.Height, layout.Layers);
        texture.AddRegion(0, 0, tileWidth, tileHeight); // Region 0 is a blank one for unmapped sub-ids

        int regionNum = 1;
        LoadRegions(texture, dayInfo, layout, context.Disk, tileWidth, tileHeight, ref regionNum);

        if (nightInfo != null)
            LoadRegions(texture, nightInfo, layout, context.Disk, tileWidth, tileHeight, ref regionNum);

        return new TrueColorTileGraphics(texture, dayInfo, nightInfo);
    }

    class FrameComparer : IComparer<FrameInfo>
    {
        public static FrameComparer Instance { get; } = new();
        FrameComparer() { }

        public int Compare(FrameInfo x, FrameInfo y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            var subIdComparison = x.SubId.CompareTo(y.SubId);
            if (subIdComparison != 0) return subIdComparison;
            return x.PalFrame.CompareTo(y.PalFrame);
        }
    }

    static List<TileFrameSummary> GetInfo(string dir, IFileSystem disk, AssetPathPattern pattern)
    {
        var frames = new List<FrameInfo>();
        foreach (var path in disk.EnumerateDirectory(dir, "*.png"))
        {
            if (!pattern.TryParse(Path.GetFileName(path), out var assetPath))
                continue;

            frames.Add(new FrameInfo(path, assetPath.SubAsset, assetPath.PaletteFrame ?? 0));
        }

        frames.Sort(FrameComparer.Instance);
        var results = new List<TileFrameSummary>();
        int lastSubId = 0;
        var palFrames = new List<FrameInfo>();
        foreach (var frame in frames)
        {
            if (lastSubId != frame.SubId)
            {
                AddAtIndex(results, new TileFrameSummary(palFrames.Select(x => x.Path).ToArray()), null, lastSubId);
                palFrames.Clear();
            }
            else
            {
                if (frame.PalFrame != palFrames.Count)
                {
                    if (palFrames.Count == 0)
                        throw new FileNotFoundException($"Expected an image for palette frame {palFrames.Count}, but the first image for tile image {frame.SubId} was {frame.Path}");
                    throw new FileNotFoundException($"Expected an image for palette frame {palFrames.Count} between {palFrames[^1].Path} and {frame.Path}");
                }

            }

            palFrames.Add(frame);
            lastSubId = frame.SubId;
        }

        if (palFrames.Count > 0)
            AddAtIndex(results, new TileFrameSummary(palFrames.Select(x => x.Path).ToArray()), null, lastSubId);

        return results;
    }

    static void AddAtIndex<T>(List<T> list, T element, T defaultElement, int index)
    {
        while (list.Count <= index)
            list.Add(defaultElement);
        list[index] = element;
    }


    static (List<TileFrameSummary> dayInfo, List<TileFrameSummary> nightInfo) FindFiles(AssetInfo info, IFileSystem disk)
    {
        var pattern = info.GetPattern(AssetProperty.Pattern, "{ignorenum}_{frame:0000}_{palframe:0000}.png");
        var dayPath = info.Get<string>(AssetProperty.DayPath, null);
        var nightPath = info.Get<string>(AssetProperty.NightPath, null);

        if (!string.IsNullOrEmpty(dayPath))
            dayPath = Path.Combine(info.File.Filename, dayPath);

        if (!string.IsNullOrEmpty(nightPath))
            nightPath = Path.Combine(info.File.Filename, nightPath);

        var dayInfo = GetInfo(dayPath, disk, pattern);
        var nightInfo = nightPath != null ? GetInfo(nightPath, disk, pattern) : null;
        return (dayInfo, nightInfo);
    }

    void LoadRegions(
        ArrayTexture<uint> texture,
        List<TileFrameSummary> info,
        SpriteSheetLayout layout,
        IFileSystem disk,
        int tileWidth,
        int tileHeight,
        ref int regionNum)
    {
        foreach (var frame in info)
        {
            frame.RegionOffset = regionNum;

            foreach (var path in frame.Paths)
            {
                var region = layout.Positions[regionNum];
                texture.AddRegion(region.X, region.Y, tileWidth, tileHeight, region.Layer);

                using var png = LoadPng(path, disk);

                if (tileWidth != png.Width || tileHeight != png.Height)
                    throw new InvalidOperationException($"Expected tiles to be {tileWidth} x {tileHeight}, but {path} is {png.Width} x {png.Height}");

                if (!png.TryGetSinglePixelSpan(out Span<Rgba32> rgbaSpan))
                    throw new InvalidOperationException("Could not retrieve single span from Image");

                var fromSpan = MemoryMarshal.Cast<Rgba32, uint>(rgbaSpan);
                var from = new ReadOnlyImageBuffer<uint>(png.Width, png.Height, png.Width, fromSpan);
                var to = texture.GetMutableRegionBuffer(regionNum);
                BlitUtil.BlitDirect(from, to);

                regionNum++;
            }
        }
    }

    Image<Rgba32> LoadPng(string path, IFileSystem disk)
    {
        using var stream = disk.OpenRead(path);
        return _decoder.Decode<Rgba32>(_configuration, stream);
    }


    (int w, int h) GetPngSize(string path, IFileSystem disk)
    {
        using var png = LoadPng(path, disk);
        return (png.Width, png.Height);
    }
}
