using System;
using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid;

/// <summary>
/// A dummy renderable object to trigger drawing of the ImGui UI
/// </summary>
public class DebugGuiRenderable : Component, IRenderable, IRenderableSource
{
    public string Name => "DebugGui";
    public DrawLayer RenderOrder => DrawLayer.Debug;
    public void Collect(List<IRenderable> renderables)
    {
        ArgumentNullException.ThrowIfNull(renderables);
        renderables.Add(this);
    }
}