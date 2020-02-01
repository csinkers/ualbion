using System;
using System.Numerics;

namespace UAlbion.Core
{
    public interface IRenderable
    {
        string Name { get; }
        int RenderOrder { get; set; }
        Type Renderer { get; }
        Matrix4x4 Transform { get; }
    }

    // If a renderable implements this interface, then the ModelView matrix will
    // only be built from the model matrix
    public interface IScreenSpaceRenderable { }

    public interface IPositionedRenderable : IRenderable
    {
        Vector3 Position { get; set; }
    }
}
