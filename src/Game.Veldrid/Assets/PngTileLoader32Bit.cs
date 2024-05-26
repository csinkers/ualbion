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
using UAlbion.Config.Properties;
using UAlbion.Core;
using UAlbion.Core.Veldrid;
using UAlbion.Formats.Assets.Maps;
using Veldrid;

namespace UAlbion.Game.Veldrid.Assets;

public class PngTileLoader32Bit : Component, IAssetLoader<ITileGraphics>
{
    const PixelFormat TextureFormat = PixelFormat.R8_G8_B8_A8_UNorm;
    public static readonly StringAssetProperty DayPath = new("DayPath"); 
    public static readonly StringAssetProperty NightPath = new("NightPath"); 
    sealed record FrameInfo(string Path, int SubId, int PalFrame);
    readonly PngDecoderOptions _pngOptions = new();

    public object Serdes(object existing, ISerializer s, AssetLoadContext context)
        => Serdes((ITileGraphics)existing, s, context);

    public ITileGraphics Serdes(ITileGraphics existing, ISerializer s, AssetLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (s.IsWriting())
            throw new NotSupportedException("Saving png tile graphics is not currently supported");

        return Load(context);
    }

    TrueColorTileGraphics Load(AssetLoadContext context)
    {
        var engine = (Engine)Resolve<IEngine>();
        var (dayInfo, nightInfo) = FindFiles(context);

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

        ReadOnlyImageBuffer<uint> AccessRegion(LazyTexture<uint> texture, Region region, object regionContext)
        {
            var path = (string)regionContext;
            var disk = context.Disk;

            if (path == null)
                return new ReadOnlyImageBuffer<uint>(tileWidth, tileHeight, tileWidth, new uint[tileWidth * tileHeight]);

            using var png = LoadPng(path, disk);

            if (tileWidth != png.Width || tileHeight != png.Height)
                throw new InvalidOperationException($"Expected tiles to be {tileWidth} x {tileHeight}, but {path} is {png.Width} x {png.Height}");

            if (!png.DangerousTryGetSinglePixelMemory(out var rgbaMemory))
                throw new InvalidOperationException("Could not retrieve single span from Image");

            return new ReadOnlyImageBuffer<uint>(png.Width, png.Height, png.Width, MemoryMarshal.Cast<Rgba32, uint>(rgbaMemory.Span));
        }

        var texture = new LazyTexture<uint>(AccessRegion, context.AssetId, context.ToString(), layout.Width, layout.Height, layout.Layers);
        texture.AddRegion(null, 0, 0, tileWidth, tileHeight); // Region 0 is a blank one for unmapped sub-ids

        int regionNum = 1;
        LoadRegions(texture, dayInfo, layout, tileWidth, tileHeight, ref regionNum);

        if (nightInfo != null)
            LoadRegions(texture, nightInfo, layout, tileWidth, tileHeight, ref regionNum);

        return new TrueColorTileGraphics(texture, dayInfo, nightInfo);
    }

    sealed class FrameComparer : IComparer<FrameInfo>
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

    static List<TileFrameSummary> GetInfo(string dir, AssetType assetType, IFileSystem disk, AssetPathPattern pattern)
    {
        var frames = new List<FrameInfo>();
        foreach (var path in disk.EnumerateFiles(dir, "*.png"))
        {
            if (!pattern.TryParse(Path.GetFileName(path), assetType, out var assetPath))
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


    static (List<TileFrameSummary> dayInfo, List<TileFrameSummary> nightInfo) FindFiles(AssetLoadContext context)
    {
        var pattern = context.GetProperty(AssetProps.Pattern, AssetPathPattern.Build("{ignorenum}_{frame:0000}_{palframe:0000}.png"));
        var dayPath = context.GetProperty(DayPath);
        var nightPath = context.GetProperty(NightPath);

        if (!string.IsNullOrEmpty(dayPath))
            dayPath = Path.Combine(context.Filename, dayPath);

        if (!string.IsNullOrEmpty(nightPath))
            nightPath = Path.Combine(context.Filename, nightPath);

        var assetType = context.AssetId.Type;
        var dayInfo = GetInfo(dayPath, assetType, context.Disk, pattern);
        var nightInfo = nightPath != null ? GetInfo(nightPath, assetType, context.Disk, pattern) : null;
        return (dayInfo, nightInfo);
    }

    static void LoadRegions(
        LazyTexture<uint> texture,
        List<TileFrameSummary> info,
        SpriteSheetLayout layout,
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
                texture.AddRegion(path, region.X, region.Y, tileWidth, tileHeight, region.Layer);
                regionNum++;
            }
        }
    }

    Image<Rgba32> LoadPng(string path, IFileSystem disk)
    {
        using var stream = disk.OpenRead(path);
        return PngDecoder.Instance.Decode<Rgba32>(_pngOptions, stream);
    }


    (int w, int h) GetPngSize(string path, IFileSystem disk)
    {
        using var png = LoadPng(path, disk);
        return (png.Width, png.Height);
    }
}
