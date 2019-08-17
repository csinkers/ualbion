using System;
using System.Collections.Generic;
using Veldrid;

namespace UAlbion.Core
{
    public interface IRenderer : IDisposable
    {
        RenderPasses RenderPasses { get; }
        void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc);
        IEnumerable<IRenderable> UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc, IEnumerable<IRenderable> renderables);
        void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass, IRenderable renderable);
        void DestroyDeviceObjects();
    }
}