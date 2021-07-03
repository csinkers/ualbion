using System;
using UAlbion.Api.Visual;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using Veldrid;

namespace UAlbion.Core.Veldrid
{
    public class EtmWindow : Component, IRenderable
    {
        public string Name { get; }
        public DrawLayer RenderOrder { get; }
        public MultiBuffer<ushort> ActiveInstances { get; }
        public ExtrudedTilemap Tilemap { get; }
        public int ActiveCount { get; private set; }

        public EtmWindow(string name, ExtrudedTilemap tilemap, int maxCount, bool translucent)
        {
            Name = name;
            RenderOrder = translucent ? DrawLayer.TranslucentTerrain : DrawLayer.OpaqueTerrain;
            Tilemap = tilemap ?? throw new ArgumentNullException(nameof(tilemap));
            ActiveInstances = new MultiBuffer<ushort>(maxCount, BufferUsage.VertexBuffer, $"B:EtmActive_{name}");
            ActiveCount = maxCount;
            AttachChild(ActiveInstances);
            On<RenderEvent>(_ =>
            {
                // var camera = Resolve<ICamera>();
                // var frustum = new BoundingFrustum(camera.ProjectionMatrix * camera.ViewMatrix);
                // TODO: Frustum culling? occlusion culling? worth bothering?
                // TODO: Sort
            });
        }
    }
}