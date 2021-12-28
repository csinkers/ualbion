using System;
using System.Collections.Generic;

namespace UAlbion.Api.Visual;

public interface ITexture
{
    IAssetId Id { get; }
    string Name { get; }
    int Width { get; }
    int Height { get; }
    int ArrayLayers { get; }
    int SizeInBytes { get; }
    IReadOnlyList<Region> Regions { get; }
    TextureDirtyType DirtyType { get; }
    int DirtyId { get; }
    void Clean();
}

public interface IDepthTexture : ITexture
{
    int Depth { get; }
}

public interface IReadOnlyTexture<T> : ITexture where T : unmanaged
{
    ReadOnlySpan<T> PixelData { get; }
    ReadOnlyImageBuffer<T> GetRegionBuffer(int i);
    ReadOnlyImageBuffer<T> GetLayerBuffer(int i);
}

public interface IMutableTexture<T> : IReadOnlyTexture<T> where T : unmanaged
{
    Span<T> MutablePixelData { get; }
    ImageBuffer<T> GetMutableRegionBuffer(int i);
    ImageBuffer<T> GetMutableLayerBuffer(int i);
}