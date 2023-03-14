using System;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public interface IRenderer
{
    Type[] HandledTypes { get; }
    void Render(IRenderable renderable, CommandList cl, GraphicsDevice device, IResourceSetHolder set1, IResourceSetHolder set2);
}
