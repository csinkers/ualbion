using System;
using Veldrid.Utilities;

namespace UAlbion.Core
{
    public interface IRenderable
    {
        string Name { get; }
        int RenderOrder { get; }
        Type Renderer { get; }
        BoundingBox? Extents { get; }
        event EventHandler ExtentsChanged;
    }
}
