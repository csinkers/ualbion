using System.Threading;

namespace UAlbion.Game;

public static class AlbionRandom
{
    static readonly Lock SyncRoot = new();
    static uint _seed = 1;

    public static ushort Next()
    {
        lock (SyncRoot)
        {
            _seed = _seed * 0x41c64e6d + 0x3039;
            return (ushort)((_seed >> 0x10) & 0x7fff);
        }
    }
}