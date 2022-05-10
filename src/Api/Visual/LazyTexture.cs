using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace UAlbion.Api.Visual;

public class LazyTexture<T> : IReadOnlyTexture<T> where T : unmanaged
{
    public delegate ReadOnlyImageBuffer<T> RegionAccessor(LazyTexture<T> texture, Region region, object context);
    readonly List<Region> _regions = new();
    readonly List<object> _regionContexts = new();
    readonly RegionAccessor _regionAccessor;

    public LazyTexture(RegionAccessor regionAccessor, IAssetId id, int width, int height, int layers = 1)
    {
        _regionAccessor = regionAccessor ?? throw new ArgumentNullException(nameof(regionAccessor));
        Id = id;
        Name = id?.ToString();
        Width = width;
        Height = height;
        ArrayLayers = layers;
    }

    public LazyTexture(RegionAccessor regionAccessor, IAssetId id, string name, int width, int height, int layers = 1)
    {
        _regionAccessor = regionAccessor ?? throw new ArgumentNullException(nameof(regionAccessor));
        Id = id;
        Name = name;
        Width = width;
        Height = height;
        ArrayLayers = layers;
    }

    public LazyTexture<T> AddRegion(object context, int x, int y, int w, int h, int layer = 0)
    {
        _regions.Add(new Region(x, y, w, h, Width, Height, layer));
        _regionContexts.Add(context);
        return this;
    }

    public LazyTexture<T> AddRegion(object context, Vector2 offset, Vector2 size, int layer)
    {
        _regions.Add(new Region(offset, size, new Vector2(Width, Height), layer));
        _regionContexts.Add(context);
        return this;
    }

    [JsonIgnore] public IAssetId Id { get; }
    public string Name { get; }
    public int Width { get; }
    public int Height { get; }
    public int ArrayLayers { get; }
    [JsonIgnore] public int SizeInBytes => Width * Height * ArrayLayers * Unsafe.SizeOf<T>();
    public IReadOnlyList<Region> Regions => _regions;
    [JsonIgnore] public TextureDirtyType DirtyType { get; private set; }
    [JsonIgnore] public int DirtyId => 0;

    public void Clean() => DirtyType = TextureDirtyType.All;
    public override string ToString() => $"LazyTexture {Id} {Width}x{Height} ({Regions.Count} sub-images)";
    public ReadOnlyImageBuffer<T> GetRegionBuffer(int i) => _regionAccessor(this, _regions[i], _regionContexts[i]);
    public ReadOnlyImageBuffer<T> GetLayerBuffer(int i) => throw new NotSupportedException();
    [JsonIgnore] public ReadOnlySpan<T> PixelData => throw new NotSupportedException();
}