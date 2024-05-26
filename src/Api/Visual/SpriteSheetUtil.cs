using System;

namespace UAlbion.Api.Visual;

public delegate (int Width, int Height) GetFrameSizeMethod(int frame);
public delegate ReadOnlyImageBuffer<T> GetFrameMethod<T>(int frame);
public static class SpriteSheetUtil
{
    public static SpriteSheetLayout ArrangeSpriteSheet<T>( int frameCount, int margin, GetFrameMethod<T> getFrame) 
        => ArrangeSpriteSheet(frameCount, margin, AdaptDelegate(getFrame));

    static GetFrameSizeMethod AdaptDelegate<T>(GetFrameMethod<T> getFrame) =>
        x =>
        {
            var frame = getFrame(x);
            return (frame.Width, frame.Height);
        };

    public static SpriteSheetLayout ArrangeSpriteSheet(
        int frameCount,
        int margin,
        GetFrameSizeMethod getFrameSize)
    {
        ArgumentNullException.ThrowIfNull(getFrameSize);
        long totalPixels = 0;
        int width = 0;
        int layers = 1;

        for (int i = 0; i < frameCount; i++)
        {
            var frame = getFrameSize(i);
            totalPixels += (frame.Width + 2 * margin) * (frame.Height + 2 * margin);
            if (width < frame.Width) width = frame.Width;
        }

        int sqrtTotal = (int)Math.Sqrt(totalPixels);
        if (sqrtTotal > width)
            width = sqrtTotal;

        width = ApiUtil.NextPowerOfTwo(width);

        // First arrange to determine required size and positions, then create the image.
        var positions = new (int x, int y, int l)[frameCount];
        int rowHeight = 0;
        int curX = 0, curY = 0, curLayer = 0;
        for (var i = 0; i < frameCount; i++)
        {
            var frame = getFrameSize(i);
            int w = frame.Width + 2 * margin;
            int h = frame.Height + 2 * margin;

            if (width - (curX + w) < 0) // If no room left on this row
            {
                curX = 0;
                curY += rowHeight;
                rowHeight = 0;
            }

            positions[i] = (curX, curY, curLayer);
            curX += w;
            if (h > rowHeight)
                rowHeight = h;
        }

        if (curX > 0)
            curY += rowHeight;

        var height = ApiUtil.NextPowerOfTwo(curY);
        return new SpriteSheetLayout(width, height, layers, positions);
    }

    public static SpriteSheetLayout ArrangeUniform(
        int tileWidth, int tileHeight,
        int maxWidth, int maxHeight, int maxLayers,
        int count)
    {
        int tilesX = maxWidth / tileWidth;
        int tilesY = maxHeight / tileHeight;
        int tilesPerLayer = tilesX * tilesY;
        int totalLayers = (count + (tilesPerLayer - 1)) / tilesPerLayer;

        if (totalLayers > maxLayers)
            throw new InvalidOperationException($"Graphics backend's max texture size ({maxWidth} x {maxHeight} x {maxLayers}) is too small to hold all tile data ({count} x {tileWidth} x {tileHeight})");

        int totalWidth = maxWidth;
        int totalHeight = maxHeight;

        if (totalLayers == 1)
        {
            tilesX = ApiUtil.NextPowerOfTwo((int)Math.Ceiling(Math.Sqrt(count)));
            tilesY = (count + tilesX - 1) / tilesX;
            totalWidth = ApiUtil.NextPowerOfTwo(tilesX * tileWidth);
            totalHeight = ApiUtil.NextPowerOfTwo(tilesY * tileHeight);
        }

        var positions = new (int x, int y, int l)[count];
        int curX = 0, curY = 0, curLayer = 0;
        for (int i = 0; i < count; i++)
        {
            if (curLayer >= totalLayers)
                throw new InvalidOperationException("Reached invalid layer");

            positions[i] = (curX * tileWidth, curY * tileHeight, curLayer);

            curX++;
            if (curX == tilesX)
            {
                curX = 0;
                curY++;

                if (curY == tilesY)
                {
                    curY = 0;
                    curLayer++;
                }
            }
        }

        return new SpriteSheetLayout(totalWidth, totalHeight, totalLayers, positions);
    }
}
