using System;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    class Label : Component, IUiElement
    {
        public Label() : base(null) { }

        public Vector2 GetSize() => Vector2.Zero;

        public void Render(Rectangle position, int order, Action<IRenderable> addFunc)
        {
            throw new NotImplementedException();
        }
    }
}