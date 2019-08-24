using System;

namespace UAlbion.Core
{
    public interface IRenderable
    {
        string Name { get; }
        int RenderOrder { get; }
        Type Renderer { get; }
    }
}
