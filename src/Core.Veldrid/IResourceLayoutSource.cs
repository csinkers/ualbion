using System;
using Veldrid;

namespace UAlbion.Core.Veldrid
{
    public interface IResourceLayoutSource
    {
        ResourceLayout GetLayout(Type type, GraphicsDevice device);
    }
}