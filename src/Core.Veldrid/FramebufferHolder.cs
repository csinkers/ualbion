﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid.Events;
using Veldrid;
using VeldridGen.Interfaces;
using Component = UAlbion.Api.Eventing.Component;

namespace UAlbion.Core.Veldrid;

public record RefreshFramebuffersEvent : EventRecord;
public abstract class FramebufferHolder : Component, IFramebufferHolder
{
    sealed class TextureHolder : ITextureHolder
    {
        readonly Func<Texture> _getter;
        public TextureHolder(Func<Texture> getter) => _getter = getter ?? throw new ArgumentNullException(nameof(getter));
        public event PropertyChangedEventHandler PropertyChanged { add { } remove { } }
        public Texture DeviceTexture => _getter();
        public string Name => _getter()?.Name;
    }

    readonly Lock _syncRoot = new();
    readonly List<TextureHolder> _colorHolders = [];
    Framebuffer _framebuffer;
    TextureHolder _depthHolder;
    uint _width;
    uint _height;

    protected FramebufferHolder(string name, uint width, uint height)
    {
        On<DeviceCreatedEvent>(e => Update(e.Device));
        On<DestroyDeviceObjectsEvent>(_ => Dispose());
        On<RefreshFramebuffersEvent>(_ =>
        {
            Dispose();
            Dirty();
        });

        _width = width;
        _height = height;
        Name = name;
    }

    public string Name { get; }

    public uint Width
    {
        get => _width;
        set
        {
            if (_width == value)
                return;

            if (value == 0)
                throw new InvalidOperationException("Tried to resize framebuffer width to 0");

            _width = value;
            Dirty();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Width)));
        }
    }

    public uint Height
    {
        get => _height;
        set
        {
            if (_height == value)
                return;

            if (value == 0)
                throw new InvalidOperationException("Tried to resize framebuffer width to 0");

            _height = value;
            Dirty();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Height)));
        }
    }

    public Framebuffer Framebuffer
    {
        get => _framebuffer;
        protected set
        {
            _framebuffer = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Framebuffer)));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected override void Subscribed() => Dirty();
    protected override void Unsubscribed() => Dispose();
    protected abstract Framebuffer CreateFramebuffer(GraphicsDevice device);
    public abstract OutputDescription? OutputDescription { get; }

    public ITextureHolder DepthTexture
    {
        get
        {
            lock (_syncRoot)
                return _depthHolder ??= new TextureHolder(() => Framebuffer.DepthTarget?.Target);
        }
    }

    public ITextureHolder GetColorTexture(int index)
    {
        lock (_syncRoot)
        {
            while(_colorHolders.Count <= index)
                _colorHolders.Add(new TextureHolder(() => Framebuffer.ColorTargets[index].Target));
            return _colorHolders[index];
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        Framebuffer?.Dispose();
        Framebuffer = null;
    }

    void Dirty() => On<PrepareFrameResourcesEvent>(e => Update(e.Device));
    void Update(GraphicsDevice device)
    {
        Dispose();
        Framebuffer = CreateFramebuffer(device);
        if (Framebuffer != null)
        {
            _width = Framebuffer.Width;
            _height = Framebuffer.Height;
        }

        Off<PrepareFrameResourcesEvent>();
    }
}