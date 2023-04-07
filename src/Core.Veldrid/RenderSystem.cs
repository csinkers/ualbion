﻿using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid.Events;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public sealed class RenderSystem : Component, IRenderPipeline, IDisposable
{
    public string Name { get; init; }
    internal List<RenderPass> Passes { get; init; }
    internal List<IFramebufferHolder> Framebuffers { get; init; }
    internal IResourceProvider ResourceProvider { get; init; }
    internal Action<RenderSystem, GraphicsDevice> PreRender { get; init; }
    internal Action<RenderSystem, GraphicsDevice> PostRender { get; init; }

    CommandList _frameCommands;
    Fence _fence;
    bool _addedChildren;

    public RenderSystem(IEnumerable<IComponent> extraComponents)
    {
        foreach (var component in extraComponents)
            AttachChild(component);

        On<DeviceCreatedEvent>(e => RebuildDeviceObjects(e.Device));
        On<DestroyDeviceObjectsEvent>(_ => DestroyDeviceObjects());
    }

    void RebuildDeviceObjects(GraphicsDevice device)
    {
        if (_frameCommands != null)
            return;

        _frameCommands = device.ResourceFactory.CreateCommandList();
        _fence = device.ResourceFactory.CreateFence(false);
    }

    void DestroyDeviceObjects()
    {
        _frameCommands?.Dispose();
        _fence?.Dispose();
        _frameCommands = null;
        _fence = null;
    }

    protected override void Subscribed()
    {
        if (_addedChildren)
            return; // Already initialised?

        if (ResourceProvider is IComponent rpComponent)
            AttachChild(rpComponent);

        foreach (var framebuffer in Framebuffers)
            if (framebuffer is IComponent component)
                AttachChild(component);

        foreach (var pass in Passes)
            AttachChild(pass);

        _addedChildren = true;
    }

    public void Render(GraphicsDevice graphicsDevice)
    {
        if (graphicsDevice == null) throw new ArgumentNullException(nameof(graphicsDevice));
        if (_frameCommands == null)
            RebuildDeviceObjects(graphicsDevice);

        PreRender?.Invoke(this, graphicsDevice);

        int i = 0;
        foreach (var phase in Passes)
        {
            using (FrameEventCached(ref i, phase, (x, n) => $"{n} {Name} Render - {x.Name}"))
            {
                _frameCommands.Begin();
                phase.Render(graphicsDevice, _frameCommands, ResourceProvider?.ResourceSet);
                _frameCommands.End();
            }

            _fence.Reset();
            using (FrameEventCached(ref i, phase, (x, n) => $"{n} {Name} Submit commands - {x.Name}"))
                graphicsDevice.SubmitCommands(_frameCommands, _fence);

            using (FrameEventCached(ref i, phase, (x, n) => $"{n} {Name} Complete - {x.Name}"))
                graphicsDevice.WaitForFence(_fence);
        }

        PostRender?.Invoke(this, graphicsDevice);
    }

    public void Dispose()
    {
        foreach (var child in Children)
            if (child is IDisposable disposable)
                disposable.Dispose();

        RemoveAllChildren();
        DestroyDeviceObjects();
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