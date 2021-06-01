using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace UAlbion.Api.Visual
{
    public class Texture<T> : IMutableTexture<T> where T : unmanaged
    {
        readonly T[] _pixelData;
        readonly List<Region> _regions;

        public Texture(IAssetId id, int width, int height, int layers = 1, IEnumerable<Region> regions = null)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = id.ToString();
            Width = width;
            Height = height;
            ArrayLayers = layers;
            _pixelData = new T[Width * Height * ArrayLayers];
            _regions = regions?.ToList() ?? new List<Region>();
        }

        public Texture(IAssetId id, string name, int width, int height, int layers = 1, IEnumerable<Region> regions = null)
        {
            Id = id;
            Name = name;
            Width = width;
            Height = height;
            ArrayLayers = layers;
            _pixelData = new T[Width * Height * ArrayLayers];
            _regions = regions?.ToList() ?? new List<Region>();
        }

        public Texture(IAssetId id, string name, int width, int height, int layers, ReadOnlySpan<T> pixelData, IEnumerable<Region> regions = null)
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

        public Texture<T> AddRegion(int x, int y, int w, int h, int layer = 0)
        {
            _regions.Add(new Region(x, y, w, h, Width, Height, layer));
            return this;
        }

        public Texture<T> AddRegion(Vector2 offset, Vector2 size, int layer)
        {
            _regions.Add(new Region(offset, size, new Vector2(Width, Height), layer));
            return this;
        }

        public IAssetId Id { get; }
        public string Name { get; }
        public int Width { get; }
        public int Height { get; }
        public int ArrayLayers { get; }
        public bool IsDirty { get; set; } = true;
        public int SizeInBytes => PixelData.Length * Unsafe.SizeOf<T>();
        public IReadOnlyList<Region> Regions => _regions;

        public override string ToString() => $"Image {Id} {Width}x{Height} ({Regions.Count} sub-images)";

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

        public ReadOnlySpan<T> PixelData => _pixelData;
        public ReadOnlyImageBuffer<T> GetRegionBuffer(int i)
        {
            var frame = Regions[i];
            ReadOnlySpan<T> fromSlice = _pixelData.AsSpan(frame.PixelOffset, frame.PixelLength);
            return new ReadOnlyImageBuffer<T>(frame.Width, frame.Height, Width, fromSlice);
        }

        public ReadOnlyImageBuffer<T> GetLayerBuffer(int i)
        {
            if (i >= ArrayLayers)
                throw new ArgumentOutOfRangeException($"Tried to obtain a buffer for layer {i}, but there are only {ArrayLayers}");

            ReadOnlySpan<T> fromSlice = _pixelData.AsSpan(i * Width * Height, Width * Height);
            return new ReadOnlyImageBuffer<T>(Width, Height, Width, fromSlice);
        }

        public Span<T> MutablePixelData => _pixelData;
        public ImageBuffer<T> GetMutableRegionBuffer(int i)
        {
            var frame = Regions[i];
            Span<T> fromSlice = _pixelData.AsSpan(frame.PixelOffset, frame.PixelLength);
            return new ImageBuffer<T>(frame.Width, frame.Height, Width, fromSlice);
        }

        public ImageBuffer<T> GetMutableLayerBuffer(int i)
        {
            if (i >= ArrayLayers)
                throw new ArgumentOutOfRangeException($"Tried to obtain a buffer for layer {i}, but there are only {ArrayLayers}");

            Span<T> fromSlice = _pixelData.AsSpan(i * Width * Height, Width * Height);
            return new ImageBuffer<T>(Width, Height, Width, fromSlice);
        }
    }
}