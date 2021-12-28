using System;

namespace UAlbion.Formats.Exporters.Tiled;

static class DiagonalLayout
{
    public static (int x, int y) GetPositionForIndex(int n)
    {
        int previousRoot = (int)Math.Sqrt(n);
        int previousSquare = previousRoot * previousRoot;
        int difference = n - previousSquare;
        return difference <= previousRoot
            ? (previousRoot, difference)
            : (2 * previousRoot - difference, previousRoot);
    }

    public static int GetIndexForPosition(int x, int y)
    {
        int previousRoot = Math.Max(x, y);
        int previousSquare = previousRoot * previousRoot;
        return (y <= x)
            ? previousSquare + y
            : previousSquare + 2 * previousRoot - x;
    }
}