using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;
using Veldrid;

namespace UAlbion.Game.Gui
{
    class Button : UiElement
    {
        static readonly Handler[] Handlers = { };
        public Button(StringId textId) : base(Handlers)
        {
            var text = new Text(textId).Center();
            Children.Add(new ButtonFrame(text));
        }

        public override Vector2 GetSize() => GetMaxChildSize() + new Vector2(4, 0);

        public void Select(Vector2 position, int order, Action<float, Selection> registerHit)
        {
        }

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            // TODO: Emit rectangle/border renderable & hovered state highlight renderable
            var innerExtents = new Rectangle(extents.X + 2, extents.Y, extents.Width - 4, extents.Height);
            return RenderChildren(innerExtents, order, addFunc);
        }
    }
}
