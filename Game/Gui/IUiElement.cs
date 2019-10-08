using System;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public interface IUiElement : IComponent
    {
        Vector2 GetSize();
        void Render(Rectangle extents, Action<IRenderable> addFunc);
    }
}
