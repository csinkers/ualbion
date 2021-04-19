using System;
using Veldrid;

namespace UAlbion.Core.Veldrid.Visual
{
    public class IndexBuffer : VeldridComponent
    {
        readonly string _name;
        readonly uint _size;
        DeviceBuffer _buffer;

        public IndexBuffer(string name, uint size)
        {
            _name = name;
            _size = size;
        }

        public override void CreateDeviceObjects(VeldridRendererContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            _buffer = context.GraphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(_size, BufferUsage.IndexBuffer));
            _buffer.Name = _name;
        }

        public void Update<T>(VeldridRendererContext context, uint offset, T[] source) where T : unmanaged
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if(_buffer == null) CreateDeviceObjects(context);
            context.CommandList.UpdateBuffer(_buffer, offset, source);
        }

        public override void DestroyDeviceObjects()
        {
            _buffer.Dispose();
            _buffer = null;
        }
    }
}