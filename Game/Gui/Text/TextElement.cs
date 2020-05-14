using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Text
{
    public class TextElement : UiElement
    {
        readonly TextBlock _block = new TextBlock();
        IText _source;
        Rectangle _lastExtents;
        int _lastVersion;
        int? _blockFilter;

        public TextElement(string literal)
        {
            On<BackendChangedEvent>(_ => _lastVersion = 0);
            On<SetLanguageEvent>(e => _lastVersion = 0); // Force a rebuild on next render
            _source = new DynamicText(() =>
            {
                _block.Text = literal;
                return new[] { _block };
            });
        }

        public TextElement(StringId id)
        {
            On<SetLanguageEvent>(e => _lastVersion = 0); // Force a rebuild on next render
            _source = new DynamicText(() =>
            {
                var assets = Resolve<IAssetManager>();
                var settings = Resolve<ISettings>();
                var text =  assets.LoadString(id, settings.Gameplay.Language);
                _block.Text = text;
                return new[] { _block };
            });
        }

        public TextElement(IText source)
        {
            On<SetLanguageEvent>(e => _lastVersion = 0); // Force a rebuild on next render
            _source = source;
        }
        public override string ToString() => $"TextElement \"{_source?.ToString() ?? _block?.ToString()}";
        public TextElement Bold() { _block.Style = TextStyle.Fat; return this; }
        public TextElement Color(FontColor color) { _block.Color = color; return this; }
        public TextElement Left() { _block.Alignment = TextAlignment.Left; return this; }
        public TextElement Center() { _block.Alignment = TextAlignment.Center; return this; }
        public TextElement Right() { _block.Alignment = TextAlignment.Right; return this; }
        public TextElement NoWrap() { _block.Arrangement |= TextArrangement.NoWrap; return this; }
        public TextElement Source(IText source) { _source = source; _lastVersion = 0; return this; }
        public TextElement LiteralString(string literal)
        {
            _source = new DynamicText(() =>
            {
                _block.Text = literal;
                return new[] { _block };
            });
            _lastVersion = 0;
            return this;
        }

        public int? BlockFilter
        {
            get => _blockFilter;
            set
            {
                if (_blockFilter == value)
                    return;

                _blockFilter = value;
                _lastVersion = 0;
            }
        }

        IEnumerable<TextLine> BuildLines(Rectangle extents, IEnumerable<TextBlock> blocks)
        {
            var textManager = Resolve<ITextManager>();

            var line = new TextLine();
            foreach (var block in textManager.SplitBlocksToSingleWords(blocks))
            {
                var size = textManager.Measure(block);
                if (block.Arrangement.HasFlag(TextArrangement.ForceNewLine)
                    ||
                    line.Width > 0 && line.Width + size.X > extents.Width)
                {
                    yield return line;
                    line = new TextLine();
                }

                line.Add(block, size);
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

            var filtered = _source.Get().Where(x => _blockFilter == null || x.BlockId == _blockFilter);
            foreach (var line in BuildLines(extents, filtered))
                AttachChild(line);
        }

        public override Vector2 GetSize()
        {
            Vector2 size = Vector2.Zero;
            foreach (var child in Children.OfType<IUiElement>())
            {
                var childSize = child.GetSize();
                if (childSize.X > size.X)
                    size.X = childSize.X;

                size.Y += childSize.Y;
            }

            return size;
        }

        protected override int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
        {
            Rebuild(extents);

            int maxOrder = order;
            var offset = 0;
            foreach (var line in Children.OfType<TextLine>())
            {
                var lineExtents = new Rectangle(extents.X, extents.Y + offset, extents.Width, line.Height);
                maxOrder = Math.Max(maxOrder, func(line, lineExtents, order + 1));
                offset += line.Height;
            }

            return order;
        }
    }
}
