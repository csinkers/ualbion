namespace UAlbion.Game;

public static class AnimUtil
{
    const int MaxFrameCount = 8;
    static readonly int[][] Lookups = BuildBouncyLookups(MaxFrameCount);
    static int[][] BuildBouncyLookups(int frameCount)
    {
        var result = new int[frameCount - 2][];

        for (int count = 3; count <= frameCount; count++)
        {
            var cycle = new int[2 * (count - 1)];
            int frame = 0;
            for (int i = 0; i < count; i++)
                cycle[frame++] = i;
			
            for(int i = count-2; i > 0; i--)
                cycle[frame++] = i;

            result[count - 3] = cycle;
        }

        return result;
    }

    public static int GetFrame(int tick, int frameCount, bool bouncy)
    {
        if (frameCount < 2)
            return 0;

        if (!bouncy || frameCount <= 2)
            return tick % frameCount;

        if (frameCount > MaxFrameCount)
            return 0;

        var lookup = Lookups[frameCount - 3];
        return lookup[tick % lookup.Length];
    }
}