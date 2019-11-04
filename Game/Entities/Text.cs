using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.Gui;
using Veldrid;

namespace UAlbion.Game.Entities
{
    public class Text : UiElement
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<Text, SetLanguageEvent>((x,e) => x._lastVersion = 0) // Force a rebuild on next render
        );

        readonly IList<Line> _lines = new List<Line>();
        readonly TextBlock _block = new TextBlock();
        ITextSource _source;
        int _lastVersion = 0;
        Rectangle _lastExtents;

        public Text(string literal) : base(Handlers)
        {
            _source = new DynamicText(() =>
            {
                _block.Text = literal;
                return new[] { _block };
            });
        }

        public Text(StringId id) : base(Handlers)
        {
            _source = new DynamicText(() =>
            {
                var assets = Resolve<IAssetManager>();
                var settings = Resolve<ISettings>();
                var text =  assets.LoadString(id, settings.Language);
                _block.Text = text;
                return new[] { _block };
            });
        }
        public Text(ITextSource source) : base(Handlers) { _source = source; }
        
        public Text Bold() { _block.Style = TextStyle.Fat; return this; }
        public Text Color(FontColor color) { _block.Color = color; return this; }
        public Text Left() { _block.Alignment = TextAlignment.Left; return this; }
        public Text Center() { _block.Alignment = TextAlignment.Center; return this; }
        public Text Right() { _block.Alignment = TextAlignment.Right; return this; }
        public Text LiteralString(string literal)
        {
            _source = new DynamicText(() =>
            {
                _block.Text = literal;
                return new[] { _block };
            });
            _lastVersion = 0;
            return this;
        }

        IEnumerable<Line> BuildLines(Rectangle extents)
        {
            var line = new Line();
            foreach (var child in Children.OfType<TextChunk>())
            {
                var size = child.GetSize();
                if (child.Block.ForceLineBreak || line.Width > 0 && line.Width + size.X < extents.Width)
                {
                    yield return line;
                    line = new Line();
                }

                line.Width += (int) size.X;
                line.Height = Math.Max(line.Height, (int) size.Y);
                line.Alignment = child.Block.Alignment;
                line.Chunks.Add(child);
            }
            yield return line;
        }

        void Rebuild(Rectangle extents)
        {
            if (extents == _lastExtents && _source.Version <= _lastVersion)
                return;
            _lastVersion = _source.Version;
            _lastExtents = extents;

            foreach(var child in Children)
                child.Detach();
            Children.Clear();

            foreach(var block in _source.Get())
            {
                var child = new TextChunk(block);
                Exchange.Attach(child);
                Children.Add(child);
            }

            _lines.Clear();
            foreach(var line in BuildLines(extents))
                _lines.Add(line);
        }

        class Line
        {
            public int Width { get; set; }
            public int Height { get; set; } = 8;
            public TextAlignment Alignment { get; set; }
            public IList<TextChunk> Chunks { get; } = new List<TextChunk>();
        }

        int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
        {
            Rebuild(extents);

            int maxOrder = order;
            var offset = Vector2.Zero;
            foreach (var line in _lines)
            {
                var lineExtents = line.Alignment switch
                {
                    TextAlignment.Center => new Rectangle(extents.X + (extents.Width - line.Width) / 2, extents.Y + (int)offset.Y, line.Width, line.Height),
                    TextAlignment.Right => new Rectangle(extents.X + (extents.Width - line.Width), extents.Y + (int)offset.Y, line.Width, line.Height),
                    _ => new Rectangle(extents.X, extents.Y + (int)offset.Y, line.Width, line.Height)
                };

                offset.X = 0;
                foreach (var chunk in line.Chunks)
                {
                    var size = chunk.GetSize();
                    maxOrder = Math.Max(maxOrder, func(chunk,
                        new Rectangle(
                            (int)(lineExtents.X + offset.X),
                            (int)(lineExtents.Y + lineExtents.Height - size.Y),
                            (int)size.X,
                            (int)size.Y),
                        order));
                    offset.X += size.X;
                }

                offset.Y += line.Height;
            }

            return order;
        }

        public override int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            if (!extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
                return order;
            return DoLayout(extents, order, (x, y, z) => x.Select(uiPosition, y, z, registerHitFunc));
        }

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc) => DoLayout(extents, order, (x, y, z) => x.Render(y, z, addFunc));
    }
}
