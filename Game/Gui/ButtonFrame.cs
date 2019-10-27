using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public class ButtonFrame : UiElement
    {
        public class ColorScheme
        {
            public CommonColor TopLeft { get; set; }
            public CommonColor BottomRight { get; set; }
            public CommonColor Corners { get; set; }
            public CommonColor? Background { get; set; }
            public float Alpha { get; set; }
        }
        public interface ITheme { ColorScheme GetColors(ButtonState state); }

        static readonly ITheme DefaultTheme = new ButtonTheme();

        static readonly HandlerSet Handlers = new HandlerSet(
            H<ButtonFrame, WindowResizedEvent>((x, _) => x._lastExtents = new Rectangle())
            );

        UiMultiSprite _sprite;
        Rectangle _lastExtents;
        ButtonState _state = ButtonState.Normal;
        ITheme _theme = DefaultTheme;
        int _padding = 2;

        public ITheme Theme
        {
            get => _theme;
            set { if (_theme != value) { _theme = value; _lastExtents = new Rectangle(); } }
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
            var colors = _theme.GetColors(_state);

            uint C(CommonColor color) => CommonColors.Palette[color];
            uint topLeft = C(colors.TopLeft);
            uint bottomRight = C(colors.BottomRight);
            uint corners = C(colors.Corners);
            uint? background = colors.Background.HasValue ? C(colors.Background.Value) : (uint?)null;

            var flags = (SpriteFlags.NoTransform | SpriteFlags.UsePalette | SpriteFlags.LeftAligned | SpriteFlags.NoDepthTest).SetOpacity(colors.Alpha);

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
                    flags.SetOpacity(colors.Alpha < 1.0f ? colors.Alpha / 2 : colors.Alpha)));
            }

            _sprite = new UiMultiSprite(new SpriteKey(CommonColors.BorderTexture, 0, flags))
            {
                Position = new Vector3(window.UiToNorm(new Vector2(extents.X, extents.Y)), 0),
                Instances = instances.ToArray(),
                Name = $"ButtonFrame:{State} {extents.X} {extents.Y} {extents.Width} {extents.Height}"
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
    }
}
