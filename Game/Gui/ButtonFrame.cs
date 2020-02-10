using System;
using System.Numerics;
using UAlbion.Api;
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
            H<ButtonFrame, WindowResizedEvent>((x, _) => x._lastExtents = new Rectangle()),
            H<ButtonFrame, ExchangeDisabledEvent>((x, _) => { x._sprite?.Dispose(); x._sprite = null; }));

        SpriteLease _sprite;
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

        void Rebuild(Rectangle extents, DrawLayer order)
        {
            if (_sprite != null && _lastExtents == extents && _sprite.Key.RenderOrder == order)
                return;

            _lastExtents = extents;

            var window = Resolve<IWindowManager>();
            var sm = Resolve<ISpriteManager>();
            var colors = _theme.GetColors(_state);

            uint C(CommonColor color) => CommonColors.Palette[color];
            uint topLeft = C(colors.TopLeft);
            uint bottomRight = C(colors.BottomRight);
            uint corners = C(colors.Corners);
            uint? background = colors.Background.HasValue ? C(colors.Background.Value) : (uint?)null;
            int instanceCount = background.HasValue ? 7 : 6;

            if (_sprite?.Key.RenderOrder != order || instanceCount != _sprite?.Length)
            {
                _sprite?.Dispose();

                var key = new SpriteKey(CommonColors.BorderTexture, order, SpriteKeyFlags.NoTransform | SpriteKeyFlags.NoDepthTest);
                _sprite = sm.Borrow(key, instanceCount, this);
            }

            var instances = _sprite.Access();

            var position = new Vector3(window.UiToNorm(new Vector2(extents.X, extents.Y)), 0);
            var flags = SpriteFlags.None.SetOpacity(colors.Alpha);

            instances[0] = SpriteInstanceData.TopLeft( // Top
                position,
                window.UiToNormRelative(new Vector2(extents.Width - 1, 1)),
                Vector2.Zero,
                Vector2.One,
                topLeft,
                flags);
            instances[1] = SpriteInstanceData.TopLeft( // Bottom
                position + new Vector3(window.UiToNormRelative(new Vector2(1, extents.Height - 1)), 0),
                window.UiToNormRelative(new Vector2(extents.Width - 1, 1)),
                Vector2.Zero,
                Vector2.One,
                bottomRight,
                flags);
            instances[2] = SpriteInstanceData.TopLeft( // Left
                position + new Vector3(window.UiToNormRelative(new Vector2(0, 1)), 0),
                window.UiToNormRelative(new Vector2(1, extents.Height - 2)),
                Vector2.Zero,
                Vector2.One,
                topLeft,
                flags);
            instances[3] = SpriteInstanceData.TopLeft( // Right
                position + new Vector3(window.UiToNormRelative(new Vector2(extents.Width - 1, 1)), 0),
                window.UiToNormRelative(new Vector2(1, extents.Height - 2)),
                Vector2.Zero,
                Vector2.One,
                bottomRight,
                flags);

            instances[4] = SpriteInstanceData.TopLeft( // Bottom Left Corner
                position + new Vector3(window.UiToNormRelative(new Vector2(0, extents.Height - 1)), 0),
                window.UiToNormRelative(Vector2.One),
                Vector2.Zero,
                Vector2.One,
                corners,
                flags);
            instances[5] = SpriteInstanceData.TopLeft( // Top Right Corner
                position + new Vector3(window.UiToNormRelative(new Vector2(extents.Width - 1, 0)), 0),
                window.UiToNormRelative(Vector2.One),
                Vector2.Zero,
                Vector2.One,
                corners,
                flags);

            if (background.HasValue)
            {
                instances[6] = SpriteInstanceData.TopLeft( // Background
                    position + new Vector3(window.UiToNormRelative(new Vector2(1, 1)), 0),
                    window.UiToNormRelative(new Vector2(extents.Width - 2, extents.Height - 2)),
                    Vector2.Zero,
                    Vector2.One,
                    background.Value,
                    flags.SetOpacity(colors.Alpha < 1.0f ? colors.Alpha / 2 : colors.Alpha));
            }
        }

        public override Vector2 GetSize() => GetMaxChildSize() + _padding * 2 * Vector2.One;

        protected override int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
        {
            var innerExtents = new Rectangle(
                extents.X + _padding,
                extents.Y + _padding,
                extents.Width - _padding * 2,
                extents.Height - _padding * 2);

            return base.DoLayout(innerExtents, order + 1, func);
        }

        public override int Render(Rectangle extents, int order)
        {
            Rebuild(extents, (DrawLayer)order);
            return base.Render(extents, order);
        }

        public override int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            Rebuild(extents, (DrawLayer)order);
            return base.Select(uiPosition, extents, order, registerHitFunc);
        }
    }
}
