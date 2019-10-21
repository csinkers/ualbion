using System;
using System.Numerics;
using UAlbion.Core;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public class FixedPosition : UiElement
    {
        readonly Rectangle _extents;

        public FixedPosition(Rectangle extents, IUiElement child) : base(null)
        {
            _extents = extents;
            Children.Add(child);
        }

        public override Vector2 GetSize() => new Vector2(_extents.Width, _extents.Height);
        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc) => RenderChildren(_extents, order, addFunc);
        public override void Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc) => SelectChildren(uiPosition, _extents, order, registerHitFunc);
    }
}