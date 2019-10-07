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
    internal class Button : Component, IUiElement
    {
        static readonly Handler[] Handlers = { };
        public Button(StringId text) : base(Handlers)
        {
            Children.Add(new Text(text));
        }

        public Vector2 Size
        {
            get
            {
                Vector2 size = Vector2.Zero;
                if (Children != null)
                {
                    foreach (var child in Children.OfType<IUiElement>())
                    {
                        var childSize = child.Size;
                        if (childSize.X > size.X)
                            size.X = childSize.X;
                        if (childSize.Y > size.Y)
                            size.Y = childSize.Y;
                    }
                }

                size += 2 * Vector2.One; // One pixel border
                return size;
            }
        }

        public void Select(Vector2 position, Action<float, Selection> registerHit)
        {
        }

        public void Render(Rectangle extents, Action<IRenderable> addFunc)
        {
            // TODO: Emit rectangle/border renderable & hovered state highlight renderable
            foreach(var child in Children.OfType<IUiElement>())
                child.Render(new Rectangle(extents.X + 1, extents.Y + 1, extents.Width - 2, extents.Height - 2), addFunc);
        }
    }
}
