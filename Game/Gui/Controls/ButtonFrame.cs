using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Gui.Controls
{
    public class ButtonFrame : UiElement
    {
        public class ColorScheme
        {
            public CommonColor? TopLeft { get; set; }
            public CommonColor? BottomRight { get; set; }
            public CommonColor? Corners { get; set; }
            public CommonColor? Background { get; set; }
            public float Alpha { get; set; }
        }

        public delegate ColorScheme ThemeFunction(ButtonState state);

        SpriteLease _sprite;
        Rectangle _lastExtents;
        ButtonState _state = ButtonState.Normal;
        ThemeFunction _theme = ButtonTheme.Default;
        int _padding = 2;
        bool _visible = true;

        public ThemeFunction Theme
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

        public bool Visible
        {
            get => _visible;
            set
            {
                _visible = value;
                if(!_visible)
                {
                    _sprite?.Dispose();
                    _sprite = null;
                }
            }
        }

        public ButtonFrame(IUiElement child)
        {
            On<BackendChangedEvent>(_ => _lastExtents = new Rectangle());
            On<WindowResizedEvent>(e => _lastExtents = new Rectangle());

            if (child != null)
                Children.Add(child);
        }

        protected override void Unsubscribed()
        {
            _sprite?.Dispose();
            _sprite = null;
        }

        void Rebuild(Rectangle extents, DrawLayer order)
        {
            if (!_visible || (_sprite != null && _lastExtents == extents && _sprite.Key.RenderOrder == order))
                return;

            _lastExtents = extents;

            var window = Resolve<IWindowManager>();
            var sm = Resolve<ISpriteManager>();
            var commonColors = Resolve<ICommonColors>();
            var colors = _theme(_state);

            uint? C(CommonColor? color) => color.HasValue ? commonColors.Palette[color.Value] : (uint?)null;
            uint? topLeft = C(colors.TopLeft);
            uint? bottomRight = C(colors.BottomRight);
            uint? corners = C(colors.Corners);
            uint? background = colors.Background.HasValue ? C(colors.Background.Value) : null;
            int instanceCount =
                  (topLeft.HasValue     ? 2 : 0)
                + (bottomRight.HasValue ? 2 : 0)
                + (corners.HasValue     ? 2 : 0)
                + (background.HasValue  ? 1 : 0);

            if (_sprite?.Key.RenderOrder != order || instanceCount != _sprite?.Length)
            {
                _sprite?.Dispose();

                var key = new SpriteKey(commonColors.BorderTexture, order, SpriteKeyFlags.NoTransform | SpriteKeyFlags.NoDepthTest);
                _sprite = sm.Borrow(key, instanceCount, this);
            }

            var instances = _sprite.Access();
            var position = new Vector3(window.UiToNorm(extents.X, extents.Y), 0);
            var flags = SpriteFlags.None.SetOpacity(colors.Alpha);

            SubImage BuildSubImage(uint layer) =>
                new SubImage(Vector2.Zero, Vector2.One, Vector2.One, layer);

            int curInstance = 0;
            if (topLeft.HasValue)
            {
                instances[curInstance] = SpriteInstanceData.TopLeft( // Top
                    position,
                    window.UiToNormRelative(extents.Width - 1, 1),
                    BuildSubImage(topLeft.Value),
                    flags);
                instances[curInstance + 1] = SpriteInstanceData.TopLeft( // Left
                    position + new Vector3(window.UiToNormRelative(0, 1), 0),
                    window.UiToNormRelative(1, extents.Height - 2),
                    BuildSubImage(topLeft.Value),
                    flags);
                curInstance += 2;
            }

            if (bottomRight.HasValue)
            {
                instances[curInstance] = SpriteInstanceData.TopLeft( // Bottom
                    position + new Vector3(window.UiToNormRelative(1, extents.Height - 1), 0),
                    window.UiToNormRelative(extents.Width - 1, 1),
                    BuildSubImage(bottomRight.Value),
                    flags);
                instances[curInstance + 1] = SpriteInstanceData.TopLeft( // Right
                    position + new Vector3(window.UiToNormRelative(extents.Width - 1, 1), 0),
                    window.UiToNormRelative(1, extents.Height - 2),
                    BuildSubImage(bottomRight.Value),
                    flags);
                curInstance += 2;
            }

            if (corners.HasValue)
            {
                instances[curInstance] = SpriteInstanceData.TopLeft( // Bottom Left Corner
                    position + new Vector3(window.UiToNormRelative(0, extents.Height - 1), 0),
                    window.UiToNormRelative(Vector2.One),
                    BuildSubImage(corners.Value),
                    flags);
                instances[curInstance + 1] = SpriteInstanceData.TopLeft( // Top Right Corner
                    position + new Vector3(window.UiToNormRelative(extents.Width - 1, 0), 0),
                    window.UiToNormRelative(Vector2.One),
                    BuildSubImage(corners.Value),
                    flags);
                curInstance += 2;
            }

            if (background.HasValue)
            {
                instances[curInstance] = SpriteInstanceData.TopLeft( // Background
                    position + new Vector3(window.UiToNormRelative(1, 1), 0),
                    window.UiToNormRelative(extents.Width - 2, extents.Height - 2),
                    BuildSubImage(background.Value),
                    flags.SetOpacity(colors.Alpha < 1.0f ? colors.Alpha / 2 : colors.Alpha));
            }
        }

        public override Vector2 GetSize()
        {
            return GetMaxChildSize() + _padding * 2 * Vector2.One;
        }

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
