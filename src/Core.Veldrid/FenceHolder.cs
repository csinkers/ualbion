﻿using System;
using System.ComponentModel;
using UAlbion.Core.Veldrid.Events;
using Veldrid;
using VeldridGen.Interfaces;
using Component = UAlbion.Api.Eventing.Component;

namespace UAlbion.Core.Veldrid;

public sealed class FenceHolder : Component, IFenceHolder
{
    readonly Action<PrepareDeviceObjectsEvent> _prepareDelegate;
    Fence _fence;

    public FenceHolder(string name)
    {
        Name = name;
        _prepareDelegate = Prepare;
        On<DestroyDeviceObjectsEvent>(_ => Dispose());
    }

    public string Name { get; }
    public Fence Fence
    {
        get => _fence;
        private set
        {
            _fence = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Fence)));
        }
    }
    public event PropertyChangedEventHandler PropertyChanged;
    protected override void Subscribed() => Dispose(); // We expect it to already be null in this case, so we're just calling dispose to subscribe to the prepare event.
    protected override void Unsubscribed() => Dispose();

    void Prepare(PrepareDeviceObjectsEvent e)
    {
        if (Fence != null)
            Dispose();

        Fence = e.Device.ResourceFactory.CreateFence(false);
        Fence.Name = Name;
        Off<PrepareDeviceObjectsEvent>();
    }

    public void Dispose()
    {
        Fence?.Dispose();
        Fence = null;
        On(_prepareDelegate);
    }
}