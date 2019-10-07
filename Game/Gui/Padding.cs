using System;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public class Padding : Component, IUiElement
    {
        public Padding(int x, int y) : base(null) { Size = new Vector2(x, y); }
        public Vector2 Size { get; }
        public void Render(Rectangle extents, Action<IRenderable> addFunc) { }
    }
}