using System;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    class YesNoMessageBox : Component, IUiElement
    {
        public YesNoMessageBox() : base(null) { }
        // Label _label;

        public Vector2 GetSize() => Vector2.Zero;

        public void Render(Rectangle position, Action<IRenderable> addFunc)
        {
            throw new NotImplementedException();
        }
    }
}