using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Gui;
using Veldrid;

namespace UAlbion.Game.Entities
{
    public class UiItemSprite : UiElement
    {
        ItemSpriteId _id = ItemSpriteId.Nothing;
        Sprite<ItemSpriteId> _sprite;

        public UiItemSprite(ItemSpriteId id) : base(null)
        {
            Id = id;
        }

        public ItemSpriteId Id
        {
            get => _id;
            set
            {
                if (_id == value) return;
                _id = value;
                _sprite =
                    new Sprite<ItemSpriteId>(
                        0, (int)_id,
                        Vector3.Zero,
                        (int)DrawLayer.Interface,
                        SpriteFlags.NoTransform | SpriteFlags.LeftAligned | SpriteFlags.NoDepthTest);
            }
        }

        public bool Highlighted { get; set; }

        public override string ToString() => $"UiSprite {_id}";
        public override Vector2 GetSize() => Vector2.One * 16;
        public override int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            if (extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
                registerHitFunc(order, this);
            return order;
        }

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            if (_sprite == null)
                return order;
            var window = Resolve<IWindowManager>();
            var position = new Vector3(window.UiToNorm(new Vector2(extents.X, extents.Y)), 0);
            var size = window.UiToNormRelative(new Vector2(extents.Width, extents.Height));

            if (Highlighted)
                _sprite.Flags |= SpriteFlags.Highlight;
            else
                _sprite.Flags &= ~SpriteFlags.Highlight;

            _sprite.Position = position;
            _sprite.Size = size;
            if (_sprite.RenderOrder != order)
                _sprite.RenderOrder = order;
            addFunc(_sprite);
            return order;
        }
    }
}