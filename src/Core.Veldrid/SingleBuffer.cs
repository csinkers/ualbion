using System;
using System.Runtime.CompilerServices;
using UAlbion.Core.Veldrid.Events;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid
{
    public sealed class SingleBuffer<T> : Component, IBufferHolder<T> where T : unmanaged // GPU buffer containing a single instance of T
    {
        readonly Action<IVeldridInitEvent> _updateAction; // Cached action to avoid allocating new closures every time a buffer is dirtied
        readonly BufferUsage _usage;
        string _name;
        T _instance;

        public DeviceBuffer DeviceBuffer { get; private set; }

        public T Data
        {
            get => _instance;
            set
            {
                _instance = value;
                Dirty();
            }
        }

        public delegate void ModifierFunc(ref T data);
        public void Modify(ModifierFunc func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));
            func(ref _instance);
            Dirty();
        }


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

        public SingleBuffer(in T data, BufferUsage usage, string name = null)
        {
            _instance = data;
            _usage = usage;
            _name = name;
            _updateAction = Update;

            On<DeviceCreatedEvent>(_ => Dirty());
            On<DestroyDeviceObjectsEvent>(_ => Dispose());
        }

        public SingleBuffer(BufferUsage usage, string name = null)
        {
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
            var size = (uint)Unsafe.SizeOf<T>();
            if (DeviceBuffer != null && DeviceBuffer.SizeInBytes != size)
                Dispose();

            if (DeviceBuffer == null)
            {
                DeviceBuffer = e.Device.ResourceFactory.CreateBuffer(new BufferDescription(size, _usage));
                DeviceBuffer.Name = _name;
                GC.ReRegisterForFinalize(this);
            }

            e.CommandList.UpdateBuffer(DeviceBuffer, 0, _instance);
            Off<PrepareFrameResourcesEvent>();
        }

        public void Dispose()
        {
            DeviceBuffer?.Dispose();
            DeviceBuffer = null;
        }
    }
}
