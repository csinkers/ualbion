using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    class LoadGameScreen : Component, IUiElement
    {
        public LoadGameScreen() : base(null) { }
        readonly Button[] _slots = new Button[10];

        public Vector2 GetSize() => Vector2.Zero;

        public int Render(Rectangle position, int order, Action<IRenderable> addFunc)
        {
            throw new NotImplementedException();
        }
    }
}