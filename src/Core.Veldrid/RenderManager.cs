using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Core.Visual;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public sealed class RenderManager : Component, IDisposable
{
    internal List<IRenderer> Renderers { private get; init; }
    internal List<IRenderableSource> Sources { private get; init; }
    internal List<IFramebufferHolder> Framebuffers { private get; init; }
    internal Dictionary<string, RenderSystem> Systems { private get; init; }
    public RenderSystem GetSystem(string name) => Systems[name];

    public IFramebufferHolder GetFramebuffer(string name) => Framebuffers.Single(x => x.Name == name);

    protected override void Subscribing()
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

        foreach (var system in Systems.Values)
        {
            if (system is IComponent component)
            {
                system.IsActive = false; // Systems start inactive to avoid clashes between any ServiceComponents they provide
                AttachChild(component);
            }
        }

        base.Subscribing();
    }

    public void Dispose()
    {
        foreach(var child in Children)
            if (child is IDisposable disposable)
                disposable.Dispose();

        RemoveAllChildren();
    }
}