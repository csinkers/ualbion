using System;
using System.Runtime.InteropServices;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;
using Veldrid;

namespace UAlbion.Core.Veldrid.Sprites;

public sealed class TileLayerRenderable : Component, IRenderable, IDisposable
{
    public string Name { get; }
    public DrawLayer RenderOrder { get; }
    public MultiBuffer<GpuMapTile> Map { get; }
    internal SingleBuffer<TileLayerUniform> Uniform { get; }
    internal TileLayerResourceSet Resources { get; private set; }
    public TilesetResourceHolder Tileset { get; }
    public int FrameNumber
    {
        get => Uniform.Data.FrameNumber;
        set { Uniform.Modify(static (int v, ref TileLayerUniform x) => x.FrameNumber = v, value); }
    }

    public bool IsUnderlayOpaque
    {
        get => (Uniform.Data.LayerFlags & GpuTileLayerFlags.OpaqueUnderlay) != 0;
        set => SetFlag(GpuTileLayerFlags.OpaqueUnderlay, value);
    }
    public bool IsUnderlayActive
    {
        get => (Uniform.Data.LayerFlags & GpuTileLayerFlags.DrawUnderlay) != 0;
        set => SetFlag(GpuTileLayerFlags.DrawUnderlay, value);
    }

    public bool IsOverlayActive
    {
        get => (Uniform.Data.LayerFlags & GpuTileLayerFlags.DrawOverlay) != 0;
        set => SetFlag(GpuTileLayerFlags.DrawOverlay, value);
    }

    public TileLayerRenderable(string name, byte width, ReadOnlySpan<uint> map, DrawLayer renderOrder, TilesetResourceHolder tileset)
    {
        if (map == null) throw new ArgumentNullException(nameof(map));
        Name = name;
        RenderOrder = renderOrder;
        Tileset = tileset ?? throw new ArgumentNullException(nameof(tileset));

        var uniform = new TileLayerUniform
        {
            MapWidth = width,
            MapHeight = (uint)((map.Length + width - 1) / width),
            FrameNumber = 0,
            LayerFlags = GpuTileLayerFlags.DrawUnderlay | GpuTileLayerFlags.DrawOverlay | GpuTileLayerFlags.OpaqueUnderlay
        };

        var mapSpan = MemoryMarshal.Cast<uint, GpuMapTile>(map);
        Map = new MultiBuffer<GpuMapTile>(mapSpan, BufferUsage.StructuredBufferReadOnly, $"SB_{Name}");
        Uniform = new SingleBuffer<TileLayerUniform>(uniform, BufferUsage.UniformBuffer, $"B_{Name}Uniform");

        AttachChild(Map);
        AttachChild(Uniform);
    }

    void SetFlag(GpuTileLayerFlags flag, bool value)
    {
        Uniform.Modify(static ((GpuTileLayerFlags flag, bool v) context, ref TileLayerUniform x) =>
        {
            x.LayerFlags = context.v
                ? x.LayerFlags | context.flag
                : x.LayerFlags & ~context.flag;
        }, (flag, value));
    }

    public void SetTile(int index, uint tileNumber)
    {
        var span = Map.Borrow();
        span[index] = new GpuMapTile { Tile = tileNumber };
    }

    protected override void Subscribed()
    {
        Resources = new TileLayerResourceSet
        {
            Name = $"RS_{Name}",
            Uniform = Uniform,
            Map = Map,
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