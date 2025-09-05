using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;

namespace UAlbion.Api.Visual;

public interface ITexture
{
    [JsonIgnore]
    IAssetId Id { get; }
    string Name { get; }
    int Width { get; }
    int Height { get; }
    int ArrayLayers { get; }
    int SizeInBytes { get; }
    IReadOnlyList<Region> Regions { get; }
    int Version { get; }
}

public interface IDepthTexture : ITexture
{
    int Depth { get; }
}

public interface IReadOnlyTexture<T> : ITexture where T : unmanaged
{
    [JsonIgnore]
    ReadOnlySpan<T> PixelData { get; }
    ReadOnlyImageBuffer<T> GetRegionBuffer(Region region);
    ReadOnlyImageBuffer<T> GetRegionBuffer(int i);
    ReadOnlyImageBuffer<T> GetLayerBuffer(int i);
}

public interface IMutableTexture<T> : IReadOnlyTexture<T> where T : unmanaged
{
    [JsonIgnore]
    Span<T> MutablePixelData { get; }
    ImageBuffer<T> GetMutableRegionBuffer(Region region);
    ImageBuffer<T> GetMutableRegionBuffer(int i);
    ImageBuffer<T> GetMutableLayerBuffer(int i);
    void AddRegion(int x, int y, int w, int h, int layer = 0);
    void AddRegion(Vector2 offset, Vector2 size, int layer);
}