using System;
using System.Linq;
using System.Numerics;
using UAlbion.Game.Gui;
using Veldrid;

namespace UAlbion.Game.Entities
{
    public class TextLine : UiElement
    {
        public int Width { get; private set; }
        public int Height { get; private set; } = 8;
        TextAlignment _alignment;

        /// <summary>
        /// Add a new block to the line.
        /// </summary>
        /// <param name="block"></param>
        /// <param name="size"></param>
        public void Add(TextBlock block, Vector2 size)
        {
            if (string.IsNullOrEmpty(block.Text))
                return;

            Width += (int)size.X;
            Height = Math.Max(Height, (int)size.Y);
            _alignment = block.Alignment;

            var lastChild = Children.OfType<TextChunk>().LastOrDefault();
            if(lastChild != null && block.IsMergeableWith(lastChild.Block))
            {
                lastChild.Block.Merge(block);
                lastChild.IsDirty = true;
            }
            else
            {
                 AttachChild(new TextChunk(block));
            }
        }

        public override string ToString() => 
            "TextLine:[ " +
            string.Join("; ", Children.OfType<TextChunk>().Select(x => x.ToString()))
            + " ]";

        protected override int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
        {
            var lineExtents = _alignment switch
            {
                TextAlignment.Center => new Rectangle(extents.X + (extents.Width - Width) / 2, extents.Y, Width, Height),
                TextAlignment.Right => new Rectangle(extents.X + (extents.Width - Width), extents.Y, Width, Height),
                _ => new Rectangle(extents.X, extents.Y, Width, Height)
            };

            int maxOrder = order;
            int offset = 0;
            foreach (var chunk in Children.OfType<IUiElement>())
            {
                var size = chunk.GetSize();
                maxOrder = Math.Max(maxOrder, func(chunk,
                    new Rectangle(
                        (int)(lineExtents.X + offset),
                        (int)(lineExtents.Y + lineExtents.Height - size.Y),
                        (int)size.X,
                        (int)size.Y),
                    order));
                offset += (int)size.X;
            }

            return maxOrder;
        }
    }
}
