using System;

namespace UAlbion.Api.Visual
{
    public static class SpriteSheetUtil
    {
        public static SpriteSheetLayout ArrangeSpriteSheet<T>(int frameCount, int margin, GetFrameDelegate<T> getFrame)
        {
            long totalPixels = 0;
            int width = 0;

            for (int i = 0; i < frameCount; i++)
            {
                var frame = getFrame(i);
                totalPixels += (frame.Width + 2 * margin) * (frame.Height + 2 * margin);
                if (width < frame.Width) width = frame.Width;
            }

            int sqrtTotal = (int)Math.Sqrt(totalPixels);
            if (sqrtTotal > width)
                width = sqrtTotal;

            width = ApiUtil.NextPowerOfTwo(width);

            // First arrange to determine required size and positions, then create the image.
            var positions = new (int, int)[frameCount];
            int rowHeight = 0;
            int curX = 0, curY = 0;
            for (var i = 0; i < frameCount; i++)
            {
                var frame = getFrame(i);
                int w = frame.Width + 2 * margin;
                int h = frame.Height + 2 * margin;

                if (width - (curX + w) < 0) // If no room left on this row
                {
                    curX = 0;
                    curY += rowHeight;
                    rowHeight = 0;
                }

                positions[i] = (curX, curY);
                curX += w;
                if (h > rowHeight)
                    rowHeight = h;
            }

            if (curX > 0)
                curY += rowHeight;

            var height = curY;
            return new SpriteSheetLayout(width, height, positions);
        }
    }
}