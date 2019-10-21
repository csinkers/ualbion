using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using Veldrid;

namespace UAlbion.Game.Gui
{
    class ButtonFrame : UiElement
    {
        public enum ColorScheme
        {
            Monochrome,
            BlueGrey
        }

        static readonly IDictionary<CommonColor, uint> Palette =
            new[]
            {
                CommonColor.White,
                CommonColor.Grey8,
                CommonColor.Black2,
                CommonColor.BlueGrey3,
                CommonColor.BlueGrey4,
                CommonColor.BlueGrey6,
                CommonColor.Teal1,
                CommonColor.Teal3,
                CommonColor.Teal4,
            }.Select((x, i) => (x, i)).ToDictionary(x => x.x, x => (uint)x.i);

        static readonly ITexture BorderTexture = new EightBitTexture(
            "ButtonBorder",
            1, 1, 1, (uint)Palette.Count,
            Palette.OrderBy(x => x.Value).Select(x => (byte)x.Key).ToArray(),
            Palette.OrderBy(x => x.Value)
                .Select(x => new EightBitTexture.SubImage(0, 0, 1, 1, x.Value))
                .ToArray());

        static readonly HandlerSet Handlers = new HandlerSet(
            H<ButtonFrame, WindowResizedEvent>((x, _) => x._lastExtents = new Rectangle())
            );

        UiMultiSprite _sprite;
        Rectangle _lastExtents;
        ButtonState _state = ButtonState.Normal;
        ColorScheme _scheme = ColorScheme.Monochrome;
        int _padding = 2;

        public ColorScheme Scheme
        {
            get => _scheme;
            set { if(_scheme != value) { _scheme = value; _lastExtents = new Rectangle(); } }
        }

        public ButtonState State
        {
            get => _state;
            set { if (value != _state) { _state = value; _lastExtents = new Rectangle(); } }
        }

        public int Padding // Adjust by 1 pixel to account for the border.
        {
            get => _padding - 1;
            set { if (value != _padding - 1) { _padding = value + 1; _lastExtents = new Rectangle(); } }
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

            var window = Resolve<IWindowManager>();
            GetFrameColors(out var topLeft, out var bottomRight, out var corners, out var background, out float alpha);
            var flags = (SpriteFlags.NoTransform | SpriteFlags.UsePalette | SpriteFlags.LeftAligned | SpriteFlags.NoDepthTest).SetOpacity(alpha);

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
                    flags.SetOpacity(alpha < 1.0f ? alpha / 2 : alpha)));
            }

            _sprite = new UiMultiSprite(new SpriteKey(BorderTexture, 0, flags))
            {
                Position = new Vector3(window.UiToNorm(new Vector2(extents.X, extents.Y)), 0),
                Instances = instances.ToArray()
            };
        }

        public override Vector2 GetSize() => GetMaxChildSize() + _padding * 2 * Vector2.One;

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            Rebuild(extents);

            if (_sprite.RenderOrder != order)
                _sprite.RenderOrder = order;

            addFunc(_sprite);
            var innerExtents = new Rectangle(
                extents.X + _padding,
                extents.Y + _padding,
                extents.Width - _padding * 2,
                extents.Height - _padding * 2);

            return RenderChildren(innerExtents, order, addFunc);
        }

        public override void Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            if (!extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
                return;
            Rebuild(extents);

            var innerExtents = new Rectangle(
                extents.X + _padding,
                extents.Y + _padding,
                extents.Width - _padding * 2,
                extents.Height - _padding * 2);

            SelectChildren(uiPosition, innerExtents, order, registerHitFunc);
            registerHitFunc(order, this);
        }

        void GetFrameColors(out uint topLeft, out uint bottomRight, out uint corners, out uint? background, out float alpha)
        {
            uint C(CommonColor color) => Palette[color];
            if (_scheme == ColorScheme.Monochrome) // Used for most buttons
            {
                alpha = 0.4f;
                corners = C(CommonColor.Grey8);
                switch (_state)
                {
                    case ButtonState.Normal:
                    case ButtonState.ClickedBlurred:
                        topLeft = C(CommonColor.White);
                        bottomRight = C(CommonColor.Black2);
                        background = null;
                        break;
                    case ButtonState.Hover:
                        topLeft = C(CommonColor.White);
                        bottomRight = C(CommonColor.Black2);
                        background = C(CommonColor.White);
                        break;
                    case ButtonState.Clicked:
                    case ButtonState.Pressed:
                        topLeft = C(CommonColor.Black2);
                        bottomRight = C(CommonColor.White);
                        background = C(CommonColor.Black2);
                        break;
                    case ButtonState.HoverPressed:
                        topLeft = C(CommonColor.Black2);
                        bottomRight = C(CommonColor.White);
                        background = C(CommonColor.White);
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            else if (_scheme == ColorScheme.BlueGrey) // Used for slider thumbs
            {
                alpha = 1.0f;
                corners = C(CommonColor.BlueGrey4);
                switch (_state)
                {
                    case ButtonState.Normal:
                        topLeft = C(CommonColor.BlueGrey6);
                        bottomRight = C(CommonColor.BlueGrey3);
                        background = C(CommonColor.BlueGrey4);
                        break;
                    case ButtonState.Hover:
                        topLeft = C(CommonColor.Teal4);
                        bottomRight = C(CommonColor.Teal1);
                        background = C(CommonColor.Teal3);
                        break;
                    case ButtonState.Clicked:
                    case ButtonState.ClickedBlurred:
                    case ButtonState.Pressed:
                        topLeft = C(CommonColor.Teal4);
                        bottomRight = C(CommonColor.Teal1);
                        background = C(CommonColor.Teal3);
                        break;
                    case ButtonState.HoverPressed:
                        topLeft = C(CommonColor.Teal4);
                        bottomRight = C(CommonColor.Teal1);
                        background = C(CommonColor.Teal3);
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            else throw new ArgumentOutOfRangeException();
        }
    }
}
