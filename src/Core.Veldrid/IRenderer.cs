using System;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public interface IRenderer<in TGlobalSet, in TRenderPassSet> 
    where TGlobalSet : IResourceSetHolder 
    where TRenderPassSet : IResourceSetHolder
{
    Type[] HandledTypes { get; }
    void Render(IRenderable renderable, CommandList cl, GraphicsDevice device, TGlobalSet globalSet, TRenderPassSet renderPassSet);
}