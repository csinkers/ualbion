using System;
using UAlbion.Api;
using UAlbion.Api.Visual;

namespace UAlbion.Core.Visual
{
    public static class TextureBuilder
    {
        public static readonly bool[] Dashed = { true, true, true, true, true, false, false, false };
        public static TextureBuilder<T> Create<T>(IAssetId id, int width, int height) where T : unmanaged => new(id, width, height);
    }

    public class TextureBuilder<T> where T : unmanaged
    {
        readonly SimpleTexture<T> _texture;
        public IReadOnlyTexture<T> Texture => _texture;
        internal TextureBuilder(IAssetId id, int width, int height)
        {
            _texture = new SimpleTexture<T>(id, width, height);
            _texture.AddRegion(0, 0, width, height);
        }

        public int Width => _texture.Width;
        public int Height => _texture.Height;

        public TextureBuilder<T> HorzLine(T color, int x0, int x1, int y, bool[] pattern = null)
        {
            var buffer = _texture.GetMutableLayerBuffer(0);
            int offset = y * buffer.Stride;
            int end = Math.Min(offset + x1, offset + buffer.Stride);
            int n = 0;
            for (int i = offset + x0; i < end; i++)
            {
                if (pattern != null)
                {
                    if (n == pattern.Length) n = 0;
                    if (!pattern[n++]) continue;
                }

                buffer.Buffer[i] = color;
            }

            return this;
        }

        public TextureBuilder<T> VertLine(T color, int x, int y0, int y1, bool[] pattern = null)
        {
            var buffer = _texture.GetMutableLayerBuffer(0);
            int end = Math.Min(x + y1 * buffer.Stride, buffer.Buffer.Length);
            int n = 0;
            for (int i = x + y0 * buffer.Stride; i < end; i += buffer.Stride)
            {
                if (pattern != null)
                {
                    if (n == pattern.Length) n = 0;
                    if (!pattern[n++]) continue;
                }

                buffer.Buffer[i] = color;
            }

            return this;
        }

        public TextureBuilder<T> Corners(T color, int fraction = 4) =>
            HorzLine(color, 0, Width / fraction, 0)
            .HorzLine(color, (fraction - 1) * Width / fraction, Width, 0)

            .HorzLine(color, 0, Width / fraction, Height)
            .HorzLine(color, (fraction - 1) * Width / fraction, Width, Height)

            .VertLine(color, 0, 0, Height / fraction)
            .VertLine(color, 0, (fraction - 1) * Height / fraction, Height)

            .VertLine(color, Width, 0, Height / fraction)
            .VertLine(color, Width, (fraction - 1) * Height / fraction, Height);

        public TextureBuilder<T> Rect(T color, int x0, int y0, int x1, int y1, bool[] pattern = null) =>
            HorzLine(color,  x0, x1, y0, pattern)
            .VertLine(color, x0, y0, y1, pattern)
            .HorzLine(color, x0, x1, y1, pattern)
            .VertLine(color, x1, y0, y1, pattern);

        public TextureBuilder<T> FillAll(T color) => FillRect(color, 0, 0, Width - 1, Height - 1);
        public TextureBuilder<T> FillRect(T color, int x0, int y0, int x1, int y1)
        {
            x1 = Math.Min(x1, Width - 1);
            y1 = Math.Min(y1, Height - 1);

            var buffer = _texture.GetMutableLayerBuffer(0);
            int lineOffset = y0 * buffer.Stride + x0;
            int lineEnd    = y0 * buffer.Stride + x1;
            int endOffset  = y1 * buffer.Stride + x1;
            for (; lineOffset < endOffset; lineOffset += buffer.Stride, lineEnd += buffer.Stride)
                for (int i = lineOffset; i < lineEnd; i++)
                    buffer.Buffer[i] = color;

            return this;
        }

        public TextureBuilder<T> Border(T color, bool[] pattern = null) => Rect(color, 0, 0, Width - 1, Height - 1, pattern);

        public TextureBuilder<T> Text(string text, T color, int x, int y, ITextureBuilderFont font)
        {
            if (font == null) throw new ArgumentNullException(nameof(font));
            if (text == null)
                return this;

            const int letterGapRatio = 4;
            var toBuffer = _texture.GetMutableLayerBuffer(0);
            foreach (var c in text)
            {
                var fromBuffer = font.GetRegion(c);
                for (int j = 0; j < fromBuffer.Height; j++)
                {
                    var fromSlice = fromBuffer.Buffer.Slice(j * fromBuffer.Stride, fromBuffer.Width);
                    var toSlice = toBuffer.Buffer.Slice((y + j) * toBuffer.Stride + x, toBuffer.Width);
                    for (int i = 0; i < fromSlice.Length; i++)
                        if (!font.IsTransparent(fromSlice[i]))
                            toSlice[i] = color;
                }

                x += fromBuffer.Width + (letterGapRatio - 1 + fromBuffer.Width) / letterGapRatio;
            }

            return this;
        }
    }
}
