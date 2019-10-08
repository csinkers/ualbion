using System;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    class RightClickMenu : Component, IUiElement
    {
        public RightClickMenu() : base(null) { }

        public Vector2 GetSize() => Vector2.Zero;

        public void Render(Rectangle position, Action<IRenderable> addFunc)
        {
            throw new NotImplementedException();
        }
    }
}
