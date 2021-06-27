using System;
using Veldrid;

namespace UAlbion.Core.Veldrid
{
    public interface IResourceLayoutSource
    {
        ResourceLayout Get(Type type, GraphicsDevice device);
    }
}