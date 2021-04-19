using System;
using System.Collections.Generic;

namespace UAlbion.Core.Visual
{
    public interface IRenderer : IVisualComponent, IDisposable
    {
        Type[] RenderableTypes { get; }
        RenderPasses RenderPasses { get; }
        void UpdatePerFrameResources(
            IRendererContext context,
            IEnumerable<IRenderable> renderables,
            IList<IRenderable> results);
        void Render(IRendererContext context, RenderPasses renderPass, IRenderable renderable);
    }
}
