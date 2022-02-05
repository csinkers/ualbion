using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Api;
using UAlbion.Api.Visual;

namespace UAlbion.Game.Veldrid.Assets;

public class MultiFont
{
#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable CA2227 // Collection properties should be read only
    public class FontSize
    {
        public int X { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Spacing { get; set; }
    }

    readonly Dictionary<(int, char), int> _lookup = new();
    IReadOnlyTexture<byte> _texture;

    public string Alphabet { get; set; }
    public Dictionary<int, FontSize> Sizes { get; set; } = new();

    public static MultiFont Load(string fontInfoPath, string fontPngPath)
    {
        var json = new JsonUtil();
        var fontInfo = json.Deserialize<MultiFont>(File.ReadAllBytes(fontInfoPath));

        var image = LoadPng(fontPngPath);
        var regions = fontInfo.BuildRegions(image.Width, image.Height);

        if (!image.TryGetSinglePixelSpan(out Span<Rgba32> rgbaSpan))
            throw new InvalidOperationException("Could not retrieve single span from Image");

        var uintSpan = MemoryMarshal.Cast<Rgba32, uint>(rgbaSpan);
        var texture = new SimpleTexture<byte>(null, null, image.Width, image.Height, regions);
        for (int i = 0; i < uintSpan.Length; i++)
            texture.MutablePixelData[i] = uintSpan[i] == 0 ? (byte)0 : (byte)1;

        fontInfo._texture = texture;
        return fontInfo;
    }

    public ReadOnlyImageBuffer<byte> GetRegion(int size, char c) => _texture.GetRegionBuffer(GetOffset(size, c));
    public SingleFont GetFont(int size) => new(this, size);

    int GetOffset(int size, char c) =>
        _lookup.TryGetValue((size, c), out var offset)
            ? offset
            : throw new ArgumentOutOfRangeException($"No font data for character '{c}' with size {size}");

    static Image<Rgba32> LoadPng(string path)
    {
        var decoder = new PngDecoder();
        var configuration = new Configuration();
        using var fs = File.OpenRead(path);
        return decoder.Decode<Rgba32>(configuration, fs);
    }

    List<Region> BuildRegions(int totalWidth, int totalHeight)
    {
        var regions = new List<Region>();
        foreach (var kvp in Sizes)
        {
            int index = 0;
            foreach (var c in Alphabet)
            {
                int x = kvp.Value.X;
                int y = (kvp.Value.Height + kvp.Value.Spacing) * index;
                var region = new Region(x, y, kvp.Value.Width, kvp.Value.Height, totalWidth, totalHeight, 0);
                _lookup[(kvp.Key, c)] = regions.Count;
                regions.Add(region);
                index++;
            }
        }
        return regions;
    }
}
#pragma warning restore CA2227 // Collection properties should be read only
#pragma warning restore CA1034 // Nested types should not be visible