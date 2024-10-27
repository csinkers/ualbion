using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace UAlbion.Api.Visual;

public class SimpleTexture<T> : IMutableTexture<T> where T : unmanaged
{
    readonly T[] _pixelData;
    readonly List<Region> _regions;

    public SimpleTexture(IAssetId id, int width, int height, IEnumerable<Region> regions = null)
    {
        Id = id;
        Name = id?.ToString();
        Width = width;
        Height = height;
        _pixelData = new T[Width * Height];
        _regions = regions?.ToList() ?? new List<Region>();
    }

    public SimpleTexture(IAssetId id, string name, int width, int height, IEnumerable<Region> regions = null)
    {
        Id = id;
        Name = name;
        Width = width;
        Height = height;
        _pixelData = new T[Width * Height];
        _regions = regions?.ToList() ?? new List<Region>();
    }

    public SimpleTexture(IAssetId id, string name, int width, int height, ReadOnlySpan<T> pixelData, IEnumerable<Region> regions = null)
    {
        Id = id;
        Name = name;
        Width = width;
        Height = height;

        int pixelCount = Width * Height;
        if (pixelData.Length != pixelCount)
            throw new ArgumentException($"A span of {pixelData.Length} pixels was given to create an image of dimensions {Width}x{Height} ({pixelCount} expected)");
        _pixelData = pixelData.ToArray();
        _regions = regions?.ToList() ?? new List<Region>();
    }

    public void AddRegion(int x, int y, int w, int h, int layer = 0) => _regions.Add(new Region(x, y, w, h, Width, Height, layer));
    public void AddRegion(Vector2 offset, Vector2 size, int layer) => _regions.Add(new Region(offset, size, new Vector2(Width, Height), layer));

    [JsonIgnore] public IAssetId Id { get; }
    public string Name { get; }
    public int Width { get; }
    public int Height { get; }
    [JsonIgnore] public int ArrayLayers => 1;
    [JsonIgnore] public int SizeInBytes => PixelData.Length * Unsafe.SizeOf<T>();
    public IReadOnlyList<Region> Regions => _regions;
    [JsonIgnore] public int Version { get; private set; }
    [JsonIgnore] public ReadOnlySpan<T> PixelData => _pixelData;
    [JsonIgnore] public Span<T> MutablePixelData { get { Version++; return _pixelData; } }
    public override string ToString() => $"STexture {Id} {Width}x{Height} ({Regions.Count} sub-images)";

    public ReadOnlySpan<T> GetRowSpan(int regionNumber, int row)
    {
        if (regionNumber >= Regions.Count)
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
        ArgumentOutOfRangeException.ThrowIfNotEqual(i, 0);
        return new ReadOnlyImageBuffer<T>(Width, Height, Width, PixelData);
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
        ArgumentOutOfRangeException.ThrowIfNotEqual(i, 0);
        return new ImageBuffer<T>(Width, Height, Width, MutablePixelData);
    }

    public SimpleTexture<T> Clone()
    {
        var result = new SimpleTexture<T>(Id, Width, Height, Regions);
        PixelData.CopyTo(result.MutablePixelData);
        return result;
    }
}
