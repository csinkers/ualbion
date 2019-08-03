using System;
using System.Numerics;
using Veldrid;

namespace UAlbion.Core
{
    public interface IRenderer : IDisposable
    {
        void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc);
        void UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc, IRenderable renderable);
        void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass, IRenderable renderable);
        void DestroyDeviceObjects();
        // RenderOrderKey GetRenderOrderKey(Vector3 cameraPosition);
        RenderPasses RenderPasses { get; }
    }
}