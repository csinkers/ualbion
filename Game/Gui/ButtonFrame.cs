using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using Veldrid;

namespace UAlbion.Game.Gui
{
    class ButtonFrame : UiElement
    {
        enum Color
        {
            White,
            Grey,
            Black,
            Red,
            Green
        }

        static readonly ITexture BorderTexture = new EightBitTexture(
            "ButtonBorder",
            1, 1, 1, 5,
            new[]
            {
                (byte) CommonColor.White,
                (byte) CommonColor.Grey8,
                (byte) CommonColor.Black2,
                (byte) CommonColor.Red,
                (byte) CommonColor.Green4,
            },
            new[]
            {
                new EightBitTexture.SubImage(0, 0, 1, 1, 0),
                new EightBitTexture.SubImage(0, 0, 1, 1, 1),
                new EightBitTexture.SubImage(0, 0, 1, 1, 2),
                new EightBitTexture.SubImage(0, 0, 1, 1, 3),
                new EightBitTexture.SubImage(0, 0, 1, 1, 4),
            });

        static readonly HandlerSet Handlers = new HandlerSet();
        UiMultiSprite _sprite;
        Rectangle _lastExtents;
        ButtonState _state = ButtonState.Normal;
        public ButtonState State
        {
            get => _state;
            set
            {
                if (value != _state)
                {
                    _state = value;
                    _lastExtents = new Rectangle(0, 0, 0, 0);
                }
            }
        }

        public ButtonFrame(IUiElement child) : base(Handlers)
        {
            if (child != null)
                Children.Add(child);
        }

        void Rebuild(Rectangle extents)
        {
            if (_lastExtents == extents) return;
            _lastExtents = extents;

            uint topLeft = (uint)Color.White;
            uint bottomRight = (uint)Color.Black;
            uint corners = (uint)Color.Grey;
            uint? background = null;
            switch (_state)
            {
                case ButtonState.Normal:
                    topLeft = (uint)Color.White;
                    bottomRight = (uint)Color.Black;
                    break;
                case ButtonState.Hover:
                    background = (uint)Color.White;
                    break;
                case ButtonState.Clicked:
                case ButtonState.Pressed:
                    topLeft = (uint)Color.Black;
                    bottomRight = (uint)Color.White;
                    background = (uint)Color.Black;
                    break;
                case ButtonState.HoverPressed:
                    topLeft = (uint)Color.Black;
                    bottomRight = (uint)Color.White;
                    background = (uint)Color.White;
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            var window = Exchange.Resolve<IWindowManager>();
            // TODO: Cache sprite and rebuild when necessary
            var flags = (SpriteFlags.NoTransform | SpriteFlags.UsePalette).SetOpacity(0.4f);
            var instances = new List<SpriteInstanceData>
            {
                new SpriteInstanceData( // Top
                    Vector3.Zero,
                    window.UiToNormRelative(new Vector2(extents.Width - 1, 1)),
                    Vector2.Zero,
                    Vector2.One,
                    topLeft,
                    flags),
                new SpriteInstanceData( // Bottom
                    new Vector3(window.UiToNormRelative(new Vector2(1, extents.Height - 1)), 0),
                    window.UiToNormRelative(new Vector2(extents.Width - 1, 1)),
                    Vector2.Zero,
                    Vector2.One,
                    bottomRight,
                    flags),
                new SpriteInstanceData( // Left
                    new Vector3(window.UiToNormRelative(new Vector2(0, 1)), 0),
                    window.UiToNormRelative(new Vector2(1, extents.Height - 2)),
                    Vector2.Zero,
                    Vector2.One,
                    topLeft,
                    flags),
                new SpriteInstanceData( // Right
                    new Vector3(window.UiToNormRelative(new Vector2(extents.Width - 1, 1)), 0),
                    window.UiToNormRelative(new Vector2(1, extents.Height - 2)),
                    Vector2.Zero,
                    Vector2.One,
                    bottomRight,
                    flags),

                new SpriteInstanceData( // Bottom Left Corner
                    new Vector3(window.UiToNormRelative(new Vector2(0, extents.Height - 1)), 0),
                    window.UiToNormRelative(Vector2.One),
                    Vector2.Zero,
                    Vector2.One,
                    corners,
                    flags),
                new SpriteInstanceData( // Top Right Corner
                    new Vector3(window.UiToNormRelative(new Vector2(extents.Width - 1, 0)), 0),
                    window.UiToNormRelative(Vector2.One),
                    Vector2.Zero,
                    Vector2.One,
                    corners,
                    flags),
            };

            if (background.HasValue)
            {
                instances.Add(new SpriteInstanceData( // Background
                    new Vector3(window.UiToNormRelative(new Vector2(1, 1)), 0),
                    window.UiToNormRelative(new Vector2(extents.Width - 2, extents.Height - 2)),
                    Vector2.Zero,
                    Vector2.One,
                    background.Value,
                    flags.SetOpacity(0.2f)));
            }

            _sprite = new UiMultiSprite(new SpriteKey(BorderTexture, 0, false))
            {
                Position = new Vector3(window.UiToNorm(new Vector2(extents.X, extents.Y)), 0),
                Instances = instances.ToArray(),
                Flags = SpriteFlags.LeftAligned
            };
        }

        public override Vector2 GetSize() => GetMaxChildSize() + 4 * Vector2.One;

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            Rebuild(extents);

            if (_sprite.RenderOrder != order)
                _sprite.RenderOrder = order;

            addFunc(_sprite);
            var innerExtents = new Rectangle(extents.X + 2, extents.Y + 2, extents.Width - 4, extents.Height - 4);
            return RenderChildren(innerExtents, order, addFunc);
        }

        public override void Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            if (!extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
                return;
            Rebuild(extents);

            var innerExtents = new Rectangle(extents.X + 2, extents.Y + 2, extents.Width - 4, extents.Height - 4);
            SelectChildren(uiPosition, innerExtents, order, registerHitFunc);
            registerHitFunc(order, this);
        }
    }
}