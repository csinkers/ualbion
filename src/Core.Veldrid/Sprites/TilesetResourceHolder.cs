using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Visual;
using Veldrid;

namespace UAlbion.Core.Veldrid.Sprites;

public sealed class TilesetResourceHolder : Component, IDisposable
{
    readonly SpriteSampler _sampler;
    public ITexture Texture { get; }
    public MultiBuffer<GpuTileData> Tiles { get; }
    public MultiBuffer<GpuTextureRegion> Regions { get; }
    internal SingleBuffer<TilesetUniform> Uniform { get; }
    internal TilesetResourceSet Resources { get; private set; }
    public int RefCount { get; set; } // Should only be modified by the owning TileRenderableManager

    public TilesetResourceHolder(
        Vector2 tileSize,
        GpuTileData[] tileset,
        ITexture texture,
        SpriteSampler sampler,
        bool manualBlend)
    {
        Texture = texture ?? throw new ArgumentNullException(nameof(texture));
        _sampler = sampler;

        GpuTilesetFlags flags = 0;
        if (texture is IReadOnlyTexture<byte>) flags |= GpuTilesetFlags.UsePalette;
        if (texture.ArrayLayers > 1) flags |= GpuTilesetFlags.UseArray;
        if (manualBlend) flags |= GpuTilesetFlags.UseBlend;

        var uvSize = texture.Regions[0].TexSize;
        var uniform = new TilesetUniform
        {
            Flags = flags,
            TileWorldSize = tileSize,
            TileUvSize = uvSize
        };

        Tiles = new MultiBuffer<GpuTileData>(tileset, BufferUsage.StructuredBufferReadOnly, "SB_TileInfo");
        Regions = new MultiBuffer<GpuTextureRegion>(texture.Regions.Count, BufferUsage.StructuredBufferReadOnly, "SB_TileRegions");
        Uniform = new SingleBuffer<TilesetUniform>(uniform, BufferUsage.UniformBuffer, "B_TileUniform");

        var regionSpan = Regions.Borrow();
        for (int i = 0; i < regionSpan.Length; i++)
        {
            var region = texture.Regions[i];
            regionSpan[i] = new GpuTextureRegion
            {
                Offset = new Vector4(region.TexOffset.X, region.TexOffset.Y, region.Layer, 0)
            };
        }

        AttachChild(Tiles);
        AttachChild(Regions);
        AttachChild(Uniform);
    }

    protected override void Subscribed()
    {
        var samplerSource = Resolve<ISpriteSamplerSource>();
        bool isArray = (Uniform.Data.Flags & GpuTilesetFlags.UseArray) != 0;
        var source = Resolve<ITextureSource>();

        Resources = new TilesetResourceSet
        {
            Name = $"RS_TilesUnderlay:{Texture.Name}",
            Texture = isArray ? source.GetDummySimpleTexture() : source.GetSimpleTexture(Texture),
            TextureArray = isArray ? source.GetArrayTexture(Texture) : source.GetDummyArrayTexture(),
            Sampler = samplerSource.GetSampler(_sampler),
            Uniform = Uniform,
            Tiles = Tiles,
            Regions = Regions,
        };

        AttachChild(Resources);
    }

    protected override void Unsubscribed() => CleanupSets();

    void CleanupSets()
    {
        if (Resources == null) return;
        Resources.Dispose();
        RemoveChild(Resources);
        Resources = null;
    }

    public void Dispose()
    {
        CleanupSets();
        Uniform.Dispose();
    }
}