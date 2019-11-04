using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Game.Gui;
using Veldrid;

namespace UAlbion.Game.Entities
{
    public class UiSprite<T> : UiElement where T : Enum
    {
        T _id = (T)(object)-1;
        Vector2 _size;
        SpriteDefinition<T> _sprite;

        public UiSprite(T id) : base(null)
        {
            Id = id;
        }

        public T Id
        {
            get => _id;
            set
            {
                if ((int)(object)_id == (int)(object)value) return;
                _id = value;
                if ((int)(object)_id == -1)
                {
                    _sprite = null;
                    _size = Vector2.One;
                }
                else
                {
                    _sprite =
                        new SpriteDefinition<T>(
                            _id, 0,
                            Vector3.Zero,
                            (int) DrawLayer.Interface,
                            SpriteFlags.NoTransform | SpriteFlags.LeftAligned | SpriteFlags.NoDepthTest);
                    UpdateSize();
                }
            }
        }

        public bool Highlighted { get; set; }

        protected override void Subscribed()
        {
            UpdateSize();
            base.Subscribed();
        }

        void UpdateSize()
        {
            if (Exchange == null)
                return;

            var spriteResolver = Resolve<ISpriteResolver>();
            _size = spriteResolver.GetSize(typeof(T), (int)(object)_id, 0);
        }

        public override string ToString() => $"UiSprite {_id}";
        public override Vector2 GetSize() => _size;
        public override int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            if (extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
                registerHitFunc(order, this);
            return order;
        }

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            var window = Resolve<IWindowManager>();
            var position = new Vector3(window.UiToNorm(new Vector2(extents.X, extents.Y)), 0);
            var size = window.UiToNormRelative(new Vector2(extents.Width, extents.Height));

            if (Highlighted)
                _sprite.Flags |= SpriteFlags.Highlight;
            else
                _sprite.Flags &= ~SpriteFlags.Highlight;

            _sprite.Position = position;
            _sprite.Size = size;
            addFunc(_sprite);
            return order;
        }
    }
}