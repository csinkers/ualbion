using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Gui;
using Veldrid;

namespace UAlbion.Game.Entities
{
    public class TextChunk : UiElement // Renders a single TextBlock in the UI
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<TextChunk, WindowResizedEvent>((x,e) => x.IsDirty = true)
        );

        // Driving properties
        public TextBlock Block { get; }
        public bool IsDirty { get; set; }

        // Dependent properties
        IPositionedRenderable _sprite;
        Vector2 _size;

        public TextChunk(TextBlock block) : base(Handlers) { Block = block; }
        public override void Subscribed() { IsDirty = true;}
        public override string ToString() => $"TextChunk:{Block} ({_size.X}x{_size.Y})";

        void Rebuild()
        {
            if (!IsDirty)
                return;
            var textManager = Resolve<ITextManager>();
            _sprite = textManager.BuildRenderable(Block, out var size);
            _size = size;
            IsDirty = false;
        }

        public override Vector2 GetSize()
        {
            Rebuild();
            return _size;
        }

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            Rebuild();

            var window = Resolve<IWindowManager>();
            if (_sprite.RenderOrder != order)
                _sprite.RenderOrder = order;

            var newPosition = new Vector3(window.UiToNorm(new Vector2(extents.X, extents.Y)), 0);
            switch (Block.Alignment)
            {
                case TextAlignment.Left:
                    break;
                case TextAlignment.Center:
                    newPosition += 
                        new Vector3(
                            window.UiToNormRelative(new Vector2(
                                (extents.Width - _size.X) / 2,
                                (extents.Height - _size.Y) / 2)), 
                            0);
                    break;
                case TextAlignment.Right:
                    newPosition += 
                        new Vector3(
                            window.UiToNormRelative(new Vector2(
                                extents.Width - _size.X,
                                extents.Height - _size.Y)), 
                            0);
                    break;
            }

            if (_sprite.Position != newPosition) // Check first to avoid excessive triggering of the ExtentsChanged event.
                _sprite.Position = newPosition;
            addFunc(_sprite);
            return order;
        }
    }
}
