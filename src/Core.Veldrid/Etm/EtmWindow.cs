using System;
using UAlbion.Api.Visual;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using Veldrid;

namespace UAlbion.Core.Veldrid.Etm;

public class EtmWindow : Component, IRenderable
{
    int _version;

    public string Name { get; }
    public DrawLayer RenderOrder { get; }
    public MultiBuffer<ushort> ActiveInstances { get; }
    public ExtrudedTilemap Tilemap { get; }
    public int ActiveCount { get; private set; }

    public EtmWindow(string name, ExtrudedTilemap tilemap, int maxCount, bool transparent)
    {
        Name = name;
        RenderOrder = transparent ? DrawLayer.TranslucentTerrain : DrawLayer.OpaqueTerrain;
        Tilemap = tilemap ?? throw new ArgumentNullException(nameof(tilemap));
        ActiveInstances = new MultiBuffer<ushort>(maxCount, BufferUsage.VertexBuffer, $"B:EtmActive_{name}");
        ActiveCount = maxCount;
        AttachChild(ActiveInstances);
        On<RenderEvent>(e =>
        {
            if (_version >= Tilemap.Version)
                return; // Up to date

            _version = Tilemap.Version;

            if (Tilemap.TileCount != ActiveInstances.Count)
                ActiveInstances.Resize(Tilemap.TileCount);

            int j = 0;
            var active = ActiveInstances.Borrow();
            for (int i = 0; i < Tilemap.TileCount; i++)
            {
                bool isTransparent = (Tilemap.Tiles[i].Flags & DungeonTileFlags.Transparent) != 0;
                if (isTransparent == transparent)
                    active[j++] = (ushort)i;
            }

            ActiveCount = j;

            // var frustum = new BoundingFrustum(e.Camera.ProjectionMatrix * e.Camera.ViewMatrix);
            // TODO: Frustum culling? occlusion culling? worth bothering?
            // TODO: Sort
        });
    }
}