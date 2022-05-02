using System;
using System.Runtime.CompilerServices;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid.Events;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public sealed class MultiBuffer<T> : Component, IBufferHolder<T> where T : unmanaged // GPU buffer containing an array of Ts
{
    readonly object _syncRoot = new();
    readonly Action<IVeldridInitEvent> _updateAction; // Cached action to avoid allocating new closures every time a buffer is dirtied
    readonly BufferUsage _usage;
    string _name;
    T[] _buffer;

    public DeviceBuffer DeviceBuffer { get; private set; }
    public ReadOnlySpan<T> Data => _buffer;
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            if (DeviceBuffer != null)
                DeviceBuffer.Name = _name;
        }
    }

    public int Count => _buffer.Length;

    public Span<T> Borrow()
    {
        lock (_syncRoot)
        {
            Dirty();
            return _buffer;
        }
    }

    public MultiBuffer(int size, BufferUsage usage, string name = null)
    {
        _buffer = new T[size];
        _usage = usage;
        _name = name;
        _updateAction = Update;

        On<DeviceCreatedEvent>(_ => Dirty());
        On<DestroyDeviceObjectsEvent>(_ => Dispose());
    }

    public MultiBuffer(ReadOnlySpan<T> data, BufferUsage usage, string name = null)
    {
        _buffer = data.ToArray();
        _usage = usage;
        _name = name;
        _updateAction = Update;

        On<DeviceCreatedEvent>(_ => Dirty());
        On<DestroyDeviceObjectsEvent>(_ => Dispose());
    }

    protected override void Subscribed() => Dirty();
    protected override void Unsubscribed() => Dispose();
    void Dirty() => On<PrepareFrameResourcesEvent>(_updateAction);

    void Update(IVeldridInitEvent e)
    {
        var size = (uint)(_buffer.Length * Unsafe.SizeOf<T>());
        if (DeviceBuffer != null && DeviceBuffer.SizeInBytes != size)
            Dispose();

        if (Count == 0)
            return;

        if (DeviceBuffer == null)
        {
            DeviceBuffer = e.Device.ResourceFactory.CreateBuffer(new BufferDescription(size, _usage));
            DeviceBuffer.Name = _name;
        }

        // Possible improvement: for large buffers we could mark dirty intervals and only upload those rather than the whole buffer each time.
        e.CommandList.UpdateBuffer(DeviceBuffer, 0, _buffer);
        Off<PrepareFrameResourcesEvent>();
    }

    public void Resize(int size)
    {
        if (size == _buffer.Length)
            return;
        var old = _buffer;
        _buffer = new T[size];
        Array.Copy(old, _buffer, Math.Min(old.Length, _buffer.Length));
        Dirty();
    }

    public void Dispose() 
    {
        DeviceBuffer?.Dispose();
        DeviceBuffer = null;
    }
}