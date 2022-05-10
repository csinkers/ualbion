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

    public SimpleTexture<T> AddRegion(int x, int y, int w, int h, int layer = 0)
    {
        _regions.Add(new Region(x, y, w, h, Width, Height, layer));
        return this;
    }

    public SimpleTexture<T> AddRegion(Vector2 offset, Vector2 size, int layer)
    {
        _regions.Add(new Region(offset, size, new Vector2(Width, Height), layer));
        return this;
    }

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

    public ReadOnlySpan<T> GetRowSpan(int frameNumber, int row)
    {
        if(frameNumber >= Regions.Count)
            throw new ArgumentOutOfRangeException(nameof(frameNumber), $"Tried to get span for frame {frameNumber}, but the image only has {Regions.Count} sub-images");

        var frame = Regions[frameNumber];
        if (row >= frame.Height)
            throw new ArgumentOutOfRangeException(nameof(row), $"Tried to get span for row {row}, but the frame only has a height of {frame.Height}");
        int index = frame.X + Width * (frame.Y + row);
        return _pixelData.AsSpan(index, frame.Width);
    }

    public ReadOnlyImageBuffer<T> GetRegionBuffer(int i)
    {
        var frame = Regions[i];
        ReadOnlySpan<T> fromSlice = _pixelData.AsSpan(frame.PixelOffset, frame.PixelLength);
        return new ReadOnlyImageBuffer<T>(frame.Width, frame.Height, Width, fromSlice);
    }

    public ReadOnlyImageBuffer<T> GetLayerBuffer(int i)
    {
        if (i != 0) throw new ArgumentOutOfRangeException(nameof(i));
        return new ReadOnlyImageBuffer<T>(Width, Height, Width, PixelData);
    }


    public ImageBuffer<T> GetMutableRegionBuffer(int i)
    {
        if (i >= Regions.Count)
            throw new ArgumentOutOfRangeException($"Tried to obtain a buffer for region {i}, but there are only {Regions.Count}");

        Version++;
        var frame = Regions[i];
        Span<T> fromSlice = _pixelData.AsSpan(frame.PixelOffset, frame.PixelLength);
        return new ImageBuffer<T>(frame.Width, frame.Height, Width, fromSlice);
    }

    public ImageBuffer<T> GetMutableLayerBuffer(int i)
    {
        if (i != 0) throw new ArgumentOutOfRangeException(nameof(i));
        return new ImageBuffer<T>(Width, Height, Width, MutablePixelData);
    }
}