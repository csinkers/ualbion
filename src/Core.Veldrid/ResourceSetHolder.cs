using System;
using System.ComponentModel;
using UAlbion.Core.Veldrid.Events;
using Veldrid;
using VeldridGen.Interfaces;
using Component = UAlbion.Api.Eventing.Component;

namespace UAlbion.Core.Veldrid;

public abstract class ResourceSetHolder : Component, IResourceSetHolder
{
    ResourceSet _resourceSet;
    string _name;

    protected ResourceSetHolder()
    {
        On<DeviceCreatedEvent>(_ => Dirty());
        On<DestroyDeviceObjectsEvent>(_ => Dispose());
    }

    public ResourceSet ResourceSet => _resourceSet;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            if (_resourceSet != null)
                _resourceSet.Name = _name;
        }
    }

    protected override void Subscribed() 
    {
        Resubscribe();
        Dirty();
    }

    protected override void Unsubscribed() => Dispose();
    protected void Dirty() => On<PrepareFrameResourceSetsEvent>(Update);
    protected void PropertyDirty(object sender, PropertyChangedEventArgs _) => Dirty();
    protected abstract ResourceSet Build(GraphicsDevice device, ResourceLayout layout);
    protected abstract void Resubscribe();

    void Update(IVeldridInitEvent e)
    {
        bool resub = false;
        if (_resourceSet != null)
        {
            Dispose();
            resub = true;
        }

        var layoutSource = Resolve<IResourceLayoutSource>();
        var layout = layoutSource.GetLayout(GetType(), e.Device);
        _resourceSet = Build(e.Device, layout);
        _resourceSet.Name = Name;
        if (resub)
            Resubscribe();
        Off<PrepareFrameResourceSetsEvent>();
    }

    protected virtual void Dispose(bool disposing)
    {
        Off<PrepareFrameResourceSetsEvent>();
        _resourceSet?.Dispose();
        _resourceSet = null;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}