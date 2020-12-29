using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.Assets;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Text
{
    public class UiText : UiElement
    {
        const int ScrollBarWidth = 4;
        readonly IText _source;
        Rectangle _lastExtents = UiConstants.UiExtents; // Initial GetSize should give the constraint-free dimensions.
        int _lastVersion;
        int? _blockFilter;
        int _totalHeight;
        int _scrollOffset;
        bool _isScrollable;

        // public UiText() => RegisterEvents();

        public UiText(IText source)
        {
            RegisterEvents();
            _source = source;
        }

        void RegisterEvents()
        {
            On<BackendChangedEvent>(_ => _lastVersion = 0);
            On<LanguageChangedEvent>(e => _lastVersion = 0); // Force a rebuild on next render
            On<UiScrollEvent>(OnScroll);
        }

        public override string ToString() => $"UiText source:\"{_source}\"";
        // public UiText Source(IText source) { _source = source; _lastVersion = 0; return this; }
        public UiText Scrollable() { _isScrollable = true; return this; }
        public UiText Filter(int filter) { _blockFilter = filter; return this; }
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

        void OnScroll(UiScrollEvent e)
        {
            if (!_isScrollable)
                return;

            int maxScrollOffset = Math.Max(0, _totalHeight - _lastExtents.Height);
            _scrollOffset = Math.Clamp(_scrollOffset - e.Delta * 5, 0, maxScrollOffset);
        }

        IEnumerable<TextLine> BuildLines(Rectangle extents, IEnumerable<TextBlock> blocks)
        {
            var textManager = Resolve<ITextManager>();

            var line = new TextLine(_isScrollable ? extents : (Rectangle?)null);
            foreach (var block in textManager.SplitBlocksToSingleWords(blocks))
            {
                var size = textManager.Measure(block);
                bool forceNewLine = (block.ArrangementFlags & TextArrangementFlags.ForceNewLine) != 0;
                if (forceNewLine || line.Width > 0 && line.Width + size.X > extents.Width)
                {
                    if (!forceNewLine && string.IsNullOrWhiteSpace(block.Text))
                        continue;
                    yield return line;
                    line = new TextLine(_isScrollable ? extents : (Rectangle?)null);
                }

                line.Add(block, size);
            }
            yield return line;
        }

        void Rebuild(Rectangle extents)
        {
            if (_source == null || extents == _lastExtents && _source.Version <= _lastVersion)
                return;

            _lastVersion = _source.Version;
            _lastExtents = extents;

            RemoveAllChildren();

            var filtered = _source.GetBlocks().Where(x => _blockFilter == null || x.BlockId == _blockFilter);
            _totalHeight = 0;
            foreach (var line in BuildLines(extents, filtered))
            {
                _totalHeight += line.Height;
                AttachChild(line);
            }

            if (_isScrollable && _totalHeight > extents.Height)
            {
                AttachChild(new ScrollBar(
                    CommonColor.BlueGrey1,
                    ScrollBarWidth,
                    () => (_scrollOffset, _totalHeight, _lastExtents.Height)));
            }
            else _scrollOffset = 0;
        }

        public override Vector2 GetSize()
        {
            Rebuild(_lastExtents);

            Vector2 size = Vector2.Zero;
            foreach (var child in Children.OfType<IUiElement>().Where(x => x.IsActive))
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
            var offset = -_scrollOffset;
            foreach (var child in Children)
            {
                if (child is TextLine line)
                {
                    var lineExtents = new Rectangle(extents.X, extents.Y + offset, extents.Width, line.Height);
                    maxOrder = Math.Max(maxOrder, func(line, lineExtents, order + 1));
                    offset += line.Height;
                }
                else if (child is ScrollBar scrollBar)
                {
                    var scrollExtents = new Rectangle(extents.Right - scrollBar.Width, extents.Y, scrollBar.Width, extents.Height);
                    maxOrder = Math.Max(maxOrder, func(scrollBar, scrollExtents, order + 1));
                }
            }

            return order;
        }
    }
}
