using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public sealed class RenderPass : Component
{
    readonly List<IRenderable> _renderList = new();
    Dictionary<Type, IRenderer> _rendererLookup;

    public delegate void RenderMethod(RenderPass pass, GraphicsDevice device, CommandList cl, IResourceSetHolder set1);

    public string Name { get; init; }
    public List<IRenderer> Renderers { get; init; }
    public List<IRenderableSource> Sources { get; init; }
    public IFramebufferHolder Target { get; init; }
    public IResourceProvider ResourceProvider { get; init; }

    internal string[] Dependencies { get; init; }
    internal RenderMethod RenderFunc { get; init; }
    public override string ToString() => $"Pass:{Name}";

    protected override void Subscribed()
    {
        if (_rendererLookup != null)
            return;

        if (ResourceProvider is IComponent rpComponent)
            AttachChild(rpComponent);

        _rendererLookup =
            Renderers
            .SelectMany(renderer => renderer.HandledTypes.Select(type => (type, renderer)))
            .ToDictionary(x => x.type, x => x.renderer);
    }

    public void CollectAndDraw(GraphicsDevice device, CommandList cl, IResourceSetHolder set1)
    {
        _renderList.Clear();
        foreach (var source in Sources)
            source.Collect(_renderList);

        _renderList.Sort((x, y) =>
        {
            var x2 = (ushort)x.RenderOrder;
            var y2 = (ushort)y.RenderOrder;
            return x2 < y2 ? -1 : x2 > y2 ? 1 : 0;
        });

        var set2 = ResourceProvider?.ResourceSet;
        foreach (var renderable in _renderList)
            if (_rendererLookup.TryGetValue(renderable.GetType(), out var renderer))
                renderer.Render(renderable, cl, device, set1, set2);
    }

    public void Render(GraphicsDevice device, CommandList cl, IResourceSetHolder set1)
    {
        var framebuffer = Target.Framebuffer;
        if (framebuffer == null)
            throw new InvalidOperationException($"Framebuffer {Target.Name} for pass {Name} has not been created");

        cl.SetFramebuffer(Target.Framebuffer);
        RenderFunc(this, device, cl, set1);
    }
}