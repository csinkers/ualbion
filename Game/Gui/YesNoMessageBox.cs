using System;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    class YesNoMessageBox : Component, IUiElement
    {
        public YesNoMessageBox() : base(null) { }
        AlbionLabel _label;

        public Vector2 Size { get; }

        public void Render(Rectangle position, Action<IRenderable> addFunc)
        {
            throw new NotImplementedException();
        }
    }
}