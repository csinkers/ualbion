using System;

namespace UAlbion.Core
{
    public interface IRenderable
    {
        int RenderOrder { get; }
        Type Renderer { get; }
    }
}
