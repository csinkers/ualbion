using System;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    class AlbionLabel : Component, IUiElement
    {
        public AlbionLabel() : base(null) { }
        string _text;
        public Vector2 Size { get; }

        public void Render(Rectangle position, Action<IRenderable> addFunc)
        {
            throw new NotImplementedException();
        }
    }
}