using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Core.Visual;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public sealed class RenderSystem : Component, IDisposable
{
    internal List<IRenderer> Renderers { private get; init; }
    internal List<IRenderableSource> Sources { private get; init; }
    internal List<IFramebufferHolder> Framebuffers { private get; init; }
    internal Dictionary<string, RenderPipeline> Pipelines { private get; init; }

    public RenderPipeline GetPipeline(string name) => Pipelines[name];
    public IFramebufferHolder GetFramebuffer(string name) => Framebuffers.Single(x => x.Name == name);

    protected override void Subscribed()
    {
        if (Children.Count > 0)
            return;

        foreach (var renderer in Renderers)
            if (renderer is IComponent component)
                AttachChild(component);

        foreach (var source in Sources)
            if (source is IComponent component)
                AttachChild(component);

        foreach (var framebuffer in Framebuffers)
            if (framebuffer is IComponent component)
                AttachChild(component);

        foreach (var pipeline in Pipelines.Values)
            if (pipeline is IComponent component)
                AttachChild(component);

        base.Subscribed();
    }

    public void Dispose()
    {
        foreach(var child in Children)
            if (child is IDisposable disposable)
                disposable.Dispose();

        RemoveAllChildren();
        GC.SuppressFinalize(this);
    }
}