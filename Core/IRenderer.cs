using System;
using System.Collections.Generic;

namespace UAlbion.Core
{
    public interface IRenderer : IDisposable
    {
        bool CanRender(Type renderable);
        RenderPasses RenderPasses { get; }
        void CreateDeviceObjects(IRendererContext context);
        IEnumerable<IRenderable> UpdatePerFrameResources(IRendererContext context, IEnumerable<IRenderable> renderables);
        void Render(IRendererContext context, RenderPasses renderPass, IRenderable renderable);
        void DestroyDeviceObjects();
    }
}