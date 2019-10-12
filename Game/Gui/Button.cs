using System;
using System.Linq;
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

        public override Vector2 GetSize() => GetMaxChildSize() + 4 * Vector2.One;

        public void Select(Vector2 position, int order, Action<float, Selection> registerHit)
        {
        }

        public override void Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            // TODO: Emit rectangle/border renderable & hovered state highlight renderable
            foreach(var child in Children.OfType<IUiElement>())
                child.Render(
                    new Rectangle(extents.X + 2, extents.Y + 2, extents.Width - 4, extents.Height - 4),
                    order + 1, 
                    addFunc);
        }
    }
}
