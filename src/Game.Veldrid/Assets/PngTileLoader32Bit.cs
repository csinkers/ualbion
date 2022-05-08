using System;
using System.Collections.Generic;
using System.IO;
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

    static List<FrameInfo> GetInfo(string dir, IFileSystem disk, AssetPathPattern pattern)
    {
        var info = new List<FrameInfo>();
        foreach (var path in disk.EnumerateDirectory(dir, "*.png"))
        {
            if (!pattern.TryParse(Path.GetFileName(path), out var assetPath))
                continue;

            while (info.Count <= assetPath.SubAsset)
                info.Add(null);

            info[assetPath.SubAsset] = new FrameInfo(path, assetPath.SubAsset, assetPath.PaletteFrame ?? 0);
        }

        return info;
    }

    public ITileGraphics Serdes(ITileGraphics existing, AssetInfo info, ISerializer s, SerdesContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (s.IsWriting())
            throw new NotSupportedException("Saving png tile graphics is not currently supported");

        var pattern = info.GetPattern(AssetProperty.Pattern, "{ignorenum}_{frame:0000}_{palframe:0000}.png");
        var dayPath =  info.Get<string>(AssetProperty.DayPath, null);
        var nightPath = info.Get<string>(AssetProperty.NightPath, null);

        if (!string.IsNullOrEmpty(dayPath))
            dayPath = Path.Combine(info.File.Filename, dayPath);

        if (!string.IsNullOrEmpty(nightPath))
            nightPath = Path.Combine(info.File.Filename, nightPath);

        var dayInfo = GetInfo(dayPath, context.Disk, pattern);
        List<FrameInfo> nightInfo = nightPath != null ? GetInfo(nightPath, context.Disk, pattern) : null;

        var (tileWidth, tileHeight) = GetPngSize(dayInfo[0].Path, context.Disk);
        var totalPngs = dayInfo.Count + (nightInfo?.Count ?? 0);

        var limits = ((Engine)Resolve<IEngine>()).GetPixelFormatProperties(TextureFormat);
        if (limits == null)
            throw new InvalidOperationException($"Graphics backend does not support pixel format \"{TextureFormat}\"");

        int maxWidth = (int)limits.Value.MaxWidth;
        int maxHeight = (int)limits.Value.MaxHeight;
        int maxLayers = (int)limits.Value.MaxArrayLayers;
        int tilesX = maxWidth / tileWidth;
        int tilesY = maxHeight / tileHeight;
        int tilesPerLayer = tilesX * tilesY;
        int requiredLayers = (totalPngs + (tilesPerLayer - 1)) / tilesPerLayer;

        if (requiredLayers > maxLayers)
            throw new InvalidOperationException($"Graphics backend's max texture size ({maxWidth} x {maxHeight} x {maxLayers}) is too small to hold all tile data ({totalPngs} x {tileWidth} x {tileHeight})");

        int totalWidth = maxWidth;
        int totalHeight = maxHeight;

        if (requiredLayers == 1)
        {
            tilesX = ApiUtil.NextPowerOfTwo((int)Math.Ceiling(Math.Sqrt(totalPngs)));
            tilesY = totalPngs / tilesX;
            totalWidth = ApiUtil.NextPowerOfTwo(tilesX * tileWidth);
            totalHeight = ApiUtil.NextPowerOfTwo(tilesY * tileHeight);
        }

        List<(int offset, int palCount)> dayFrameInfo = new();
        List<(int offset, int palCount)> nightFrameInfo = nightInfo == null ? null : new List<(int offset, int palCount)>();

        List<Region> regions = new();
        int tx = 0;
        int ty = 0;
        int tl = 0;
        // for (int i = 0; i < dayInfo.Count; i++)
        // {
        // }

        var texture = new ArrayTexture<uint>(info.AssetId, info.ToString(), totalWidth, totalHeight, requiredLayers, regions);

        return new TrueColorTileGraphics(texture, dayFrameInfo, nightFrameInfo);
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

    public object Serdes(object existing, AssetInfo info, ISerializer s, SerdesContext context)
        => Serdes((ITileGraphics)existing, info, s, context);
}
