using System;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public class Padding : Component, IUiElement
    {
        readonly Vector2 _size;
        public Padding(int x, int y) : base(null) { _size = new Vector2(x, y); }
        public Vector2 GetSize() => _size;

        public void Render(Rectangle extents, Action<IRenderable> addFunc) { }
    }
}