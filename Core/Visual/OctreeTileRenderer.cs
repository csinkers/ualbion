using System;
using System.Collections.Generic;
using System.Linq;
using Veldrid;

namespace UAlbion.Core.Visual
{
    public class OctreeTiles : IRenderable
    {
        public int RenderOrder { get; }
        public Type Renderer => typeof(OctreeTileRenderer);
    }

    public class OctreeTileRenderer : IRenderer
    {
        public RenderPasses RenderPasses => RenderPasses.Standard;
        public void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
        }

        public IEnumerable<IRenderable> UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc, IEnumerable<IRenderable> renderables)
        {
            return Enumerable.Empty<IRenderable>();
        }

        public void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass, IRenderable renderable)
        {
        }

        public void DestroyDeviceObjects()
        {
        }

        public void Dispose()
        {
        }
    }
}
