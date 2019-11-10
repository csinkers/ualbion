using System;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public class FixedPosition : UiElement, IFixedSizeUiElement
    {
        readonly Rectangle _extents;

        public FixedPosition(Rectangle extents, IUiElement child) : base(null)
        {
            _extents = extents;
            Children.Add(child);
        }

        public override Vector2 GetSize() => new Vector2(_extents.Width, _extents.Height);
        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc) => base.Render(_extents, order, addFunc);
        public override int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc) => base.Select(uiPosition, _extents, order, registerHitFunc);
    }
}