using System;
using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid.Diag;

/// <summary>
/// A dummy renderable object to trigger drawing of the ImGui UI
/// </summary>
public class ImGuiRenderable : Component, IRenderable, IRenderableSource
{
    public string Name => "ImGui";
    public DrawLayer RenderOrder => DrawLayer.Debug;
    public void Collect(List<IRenderable> renderables)
    {
        ArgumentNullException.ThrowIfNull(renderables);
        renderables.Add(this);
    }
}