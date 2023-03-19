using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public sealed class RenderPipeline : Component, IRenderPipeline, IDisposable
{
    public string Name { get; init; }
    internal List<RenderPass> Passes { get; init; }
    internal List<IFramebufferHolder> Framebuffers { get; init; }
    internal IResourceProvider ResourceProvider { get; init; }

    protected override void Subscribed()
    {
        if (Children.Count > 0)
            return; // Already initialised?

        if (ResourceProvider is IComponent rpComponent)
            AttachChild(rpComponent);

        foreach (var framebuffer in Framebuffers)
            if (framebuffer is IComponent component)
                AttachChild(component);

        foreach (var pass in Passes)
            AttachChild(pass);
    }

    public void Render(GraphicsDevice graphicsDevice, CommandList frameCommands, FenceHolder fence)
    {
        if (graphicsDevice == null) throw new ArgumentNullException(nameof(graphicsDevice));
        if (frameCommands == null) throw new ArgumentNullException(nameof(frameCommands));
        if (fence == null) throw new ArgumentNullException(nameof(fence));

        int i = 0;
        foreach (var phase in Passes)
        {
            using (FrameEventCached(ref i, phase, (x, n) => $"{Name} {n} Render - {x.Name}"))
            {
                frameCommands.Begin();
                phase.Render(graphicsDevice, frameCommands, ResourceProvider?.ResourceSet);
                frameCommands.End();
            }

            fence.Fence.Reset();
            using (FrameEventCached(ref i, phase, (x, n) => $"{Name} {n} Submit commands - {x.Name}"))
                graphicsDevice.SubmitCommands(frameCommands, fence.Fence);

            using (FrameEventCached(ref i, phase, (x, n) => $"{Name} {n} Complete - {x.Name}"))
                graphicsDevice.WaitForFence(fence.Fence);
        }
    }

    public void Dispose()
    {
        foreach (var child in Children)
            if (child is IDisposable disposable)
                disposable.Dispose();

        RemoveAllChildren();
    }

    readonly List<string> _cachedStrings = new();
    FrameTimeTracker FrameEventCached<T>(ref int num, T context, Func<T, int, string> builder)
    {
        if (_cachedStrings.Count <= num)
            _cachedStrings.Add(builder(context, num));

        var message = _cachedStrings[num++];
        return PerfTracker.FrameEvent(message);
    }
}