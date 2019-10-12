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
    class Header : Component, IUiElement
    {
        static readonly Handler[] Handlers = { };

        public Header(StringId id) : base(Handlers)
        {
            var text = new Text(id).Bold().Center();
            Children.Add(text);
        }

        public Vector2 GetSize()
        {
            Vector2 size = Vector2.Zero;
            if (Children != null)
            {
                foreach (var child in Children.OfType<IUiElement>())
                {
                    var childSize = child.GetSize();
                    if (childSize.X > size.X) size.X = childSize.X;
                    if (childSize.Y > size.Y) size.Y = childSize.Y;
                }
            }

            return size;
        }

        public void Select(Vector2 position, int order, Action<float, Selection> registerHit)
        {
        }

        public void Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            foreach(var child in Children.OfType<IUiElement>())
                child.Render(extents, order + 1, addFunc);
        }
    }
}