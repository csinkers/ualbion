using System;
using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid;

public class DebugGuiRenderable : Component, IRenderable, IRenderableSource
{
    public DebugGuiRenderable() { }
    public string Name => "DebugGui";
    public DrawLayer RenderOrder => DrawLayer.Debug;
    public void Collect(List<IRenderable> renderables)
    {
        if (renderables == null) throw new ArgumentNullException(nameof(renderables));
        renderables.Add(this);
    }
}