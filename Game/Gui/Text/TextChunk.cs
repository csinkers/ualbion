using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Entities;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Text
{
    public class TextChunk : UiElement // Renders a single TextBlock in the UI
    {
        // Driving properties
        readonly Rectangle? _scissorRegion;
        public TextBlock Block { get; }
        public bool IsDirty { get; set; }

        // Dependent properties
        PositionedSpriteBatch _sprite;
        DrawLayer _lastOrder = DrawLayer.Interface;

        public TextChunk(TextBlock block, Rectangle? scissorRegion)
        {
            _scissorRegion = scissorRegion;
            On<BackendChangedEvent>(_ => IsDirty = true);
            On<WindowResizedEvent>(_ => IsDirty = true);
            Block = block;
        }
        protected override void Subscribed() { IsDirty = true;}
        protected override void Unsubscribed()
        {
            _sprite?.Dispose();
            _sprite = null;
        }

        public override string ToString() => _sprite == null ? $"TextChunk:{Block} (unloaded)" : $"TextChunk:{Block} ({_sprite.Size.X}x{_sprite.Size.Y})";

        void Rebuild(DrawLayer order)
        {
            if (!IsSubscribed || !IsDirty && order == _sprite?.RenderOrder)
                return;

            _sprite?.Dispose();
            _sprite = Resolve<ITextManager>().BuildRenderable(Block, order, _scissorRegion, this);
            _lastOrder = order;
            IsDirty = false;
        }

        public override Vector2 GetSize()
        {
            Rebuild(_lastOrder);
            return _sprite?.Size ?? Vector2.Zero;
        }

        public override int Render(Rectangle extents, int order)
        {
            Rebuild((DrawLayer)order);
            if (_sprite == null)
                return order;

            var window = Resolve<IWindowManager>();
            var newPosition = new Vector3(window.UiToNorm(extents.X, extents.Y), 0);

            switch (Block.Alignment)
            {
                case TextAlignment.Left:
                    break;
                case TextAlignment.Center:
                    newPosition +=
                        new Vector3(
                            window.UiToNormRelative(
                                (extents.Width - _sprite.Size.X) / 2,
                                (extents.Height - _sprite.Size.Y) / 2),
                            0);
                    break;
                case TextAlignment.Right:
                    newPosition +=
                        new Vector3(
                            window.UiToNormRelative(
                                extents.Width - _sprite.Size.X,
                                extents.Height - _sprite.Size.Y),
                            0);
                    break;
            }

            _sprite.Position = newPosition;
            return order;
        }
    }
}
