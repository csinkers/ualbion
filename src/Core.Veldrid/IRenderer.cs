using System;
using UAlbion.Api.Eventing;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public interface IRenderer : IComponent
{
    Type[] HandledTypes { get; }
    void Render(IRenderable renderable, CommonSet commonSet, IFramebufferHolder framebuffer, CommandList cl,
        GraphicsDevice device);
}