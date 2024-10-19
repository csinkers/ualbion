using System;
using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Core.Visual;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public sealed class RenderManager : ServiceComponent<IRenderManager>, IRenderManager, IDisposable
{
    internal Dictionary<string, IRenderer> Renderers { private get; init; }
    internal Dictionary<string, IRenderableSource> Sources { private get; init; }
    internal Dictionary<string, IFramebufferHolder> Framebuffers { private get; init; }
    internal Dictionary<string, RenderSystem> Systems { private get; init; }
    public IRenderSystem GetSystem(string name) => Systems[name];
    public IRenderer GetRenderer(string name) => Renderers[name];
    public IRenderableSource GetSource(string name) => Sources[name];
    public IFramebufferHolder GetFramebuffer(string name) => Framebuffers[name];

    protected override void Subscribing()
    {
        if (Children.Count > 0)
            return;

        foreach (var renderer in Renderers.Values)
            if (renderer is IComponent component)
                AttachChild(component);

        foreach (var source in Sources.Values)
            if (source is IComponent component)
                AttachChild(component);

        foreach (var framebuffer in Framebuffers.Values)
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