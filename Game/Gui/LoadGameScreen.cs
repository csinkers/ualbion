using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;

namespace UAlbion.Game.Gui
{
    class LoadGameScreen : IUiElement
    {
        readonly Button[] _slots = new Button[10];
        public IUiElement Parent { get; }
        public IList<IUiElement> Children { get; }
        public Vector2 Size { get; }
        public bool FixedSize { get; }

        public void Render(Vector2 position, Action<IRenderable> addFunc)
        {
            throw new NotImplementedException();
        }
    }
}