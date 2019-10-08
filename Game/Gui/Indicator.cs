using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    class AlbionIndicator : Component, IUiElement // Used for showing stats, health etc. Like a non-interactive slider.
    {
        public AlbionIndicator() : base(null) { }

        public Vector2 GetSize() => Vector2.Zero;

        public void Render(Rectangle position, Action<IRenderable> addFunc)
        {
            throw new NotImplementedException();
        }
    }
}