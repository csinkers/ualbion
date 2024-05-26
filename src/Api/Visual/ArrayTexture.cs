using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace UAlbion.Api.Visual;

public class ArrayTexture<T> : IMutableTexture<T> where T : unmanaged
{
    readonly T[] _pixelData;
    readonly List<Region> _regions;

    public ArrayTexture(IAssetId id, int width, int height, int layers = 1, IEnumerable<Region> regions = null)
    {
        Id = id;
        Name = id?.ToString();
        Width = width;
        Height = height;
        ArrayLayers = layers;
        _pixelData = new T[Width * Height * ArrayLayers];
        _regions = regions?.ToList() ?? new List<Region>();
    }

    public ArrayTexture(IAssetId id, string name, int width, int height, int layers = 1, IEnumerable<Region> regions = null)
    {
        Id = id;
        Name = name;
        Width = width;
        Height = height;
        ArrayLayers = layers;
        _pixelData = new T[Width * Height * ArrayLayers];
        _regions = regions?.ToList() ?? new List<Region>();
    }

    public ArrayTexture(IAssetId id, string name, int width, int height, int layers, ReadOnlySpan<T> pixelData, IEnumerable<Region> regions = null)
    {
        Id = id;
        Name = name;
        Width = width;
        Height = height;
        ArrayLayers = layers;

        int pixelCount = Width * Height * ArrayLayers;
        if (pixelData.Length != pixelCount)
            throw new ArgumentException($"A span of {pixelData.Length} pixels was given to create an image of dimensions {Width}x{Height}x{ArrayLayers} ({pixelCount} expected)");
        _pixelData = pixelData.ToArray();
        _regions = regions?.ToList() ?? new List<Region>();
    }

    public void AddRegion(int x, int y, int w, int h, int layer = 0) => _regions.Add(new Region(x, y, w, h, Width, Height, layer));
    public void AddRegion(Vector2 offset, Vector2 size, int layer) => _regions.Add(new Region(offset, size, new Vector2(Width, Height), layer));

    [JsonIgnore] public IAssetId Id { get; }
    public string Name { get; }
    public int Width { get; }
    public int Height { get; }
    public int ArrayLayers { get; }
    [JsonIgnore] public int SizeInBytes => PixelData.Length * Unsafe.SizeOf<T>();
    public IReadOnlyList<Region> Regions => _regions;
    [JsonIgnore] public int Version { get; private set; }
    [JsonIgnore] public ReadOnlySpan<T> PixelData => _pixelData;
    [JsonIgnore] public Span<T> MutablePixelData { get { Version++; return _pixelData; } }

    public override string ToString() => $"ATexture {Id} {Width}x{Height} ({Regions.Count} sub-images)";

    public ReadOnlySpan<T> GetRowSpan(int regionNumber, int row)
    {
        if(regionNumber >= Regions.Count)
            throw new ArgumentOutOfRangeException(nameof(regionNumber), $"Tried to get span for region {regionNumber}, but the image only has {Regions.Count} sub-images");

        var region = Regions[regionNumber];
        if (row >= region.Height)
            throw new ArgumentOutOfRangeException(nameof(row), $"Tried to get span for row {row}, but the region only has a height of {region.Height}");
        int index = region.X + Width * (region.Y + row);
        return _pixelData.AsSpan(index, region.Width);
    }

    public ReadOnlyImageBuffer<T> GetRegionBuffer(Region region)
    {
        ArgumentNullException.ThrowIfNull(region);
        ReadOnlySpan<T> fromSlice = _pixelData.AsSpan(region.PixelOffset, region.PixelLength);
        return new ReadOnlyImageBuffer<T>(region.Width, region.Height, Width, fromSlice);
    }

    public ReadOnlyImageBuffer<T> GetRegionBuffer(int i)
    {
        if (i >= Regions.Count)
            throw new ArgumentOutOfRangeException($"Tried to obtain a buffer for region {i}, but there are only {Regions.Count}");

        return GetRegionBuffer(Regions[i]);
    }

    public ReadOnlyImageBuffer<T> GetLayerBuffer(int i)
    {
        if (i >= ArrayLayers)
            throw new ArgumentOutOfRangeException($"Tried to obtain a buffer for layer {i}, but there are only {ArrayLayers}");

        ReadOnlySpan<T> fromSlice = _pixelData.AsSpan(i * Width * Height, Width * Height);
        return new ReadOnlyImageBuffer<T>(Width, Height, Width, fromSlice);
    }

    public ImageBuffer<T> GetMutableRegionBuffer(Region region)
    {
        ArgumentNullException.ThrowIfNull(region);
        Version++;
        Span<T> fromSlice = _pixelData.AsSpan(region.PixelOffset, region.PixelLength);
        return new ImageBuffer<T>(region.Width, region.Height, Width, fromSlice);
    }

    public ImageBuffer<T> GetMutableRegionBuffer(int i)
    {
        if (i >= Regions.Count)
            throw new ArgumentOutOfRangeException($"Tried to obtain a buffer for region {i}, but there are only {Regions.Count}");

        return GetMutableRegionBuffer(Regions[i]);
    }

    public ImageBuffer<T> GetMutableLayerBuffer(int i)
    {
        if (i >= ArrayLayers)
            throw new ArgumentOutOfRangeException($"Tried to obtain a buffer for layer {i}, but there are only {ArrayLayers}");

        Version++;
        Span<T> fromSlice = _pixelData.AsSpan(i * Width * Height, Width * Height);
        return new ImageBuffer<T>(Width, Height, Width, fromSlice);
    }
}
