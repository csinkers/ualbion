using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;

namespace UAlbion.Game.Gui
{
    class AlbionIndicator : IUiElement // Used for showing stats, health etc. Like a non-interactive slider.
    {
        public IUiElement Parent { get; }
        public IList<IUiElement> Children { get; }
        public Vector2 Size { get; }
        public bool FixedSize => false;

        public void Render(Vector2 position, Action<IRenderable> addFunc)
        {
            throw new NotImplementedException();
        }
    }
}