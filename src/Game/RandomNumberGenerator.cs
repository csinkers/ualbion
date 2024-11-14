using System;
using System.Threading;
using UAlbion.Api.Eventing;

namespace UAlbion.Game;

public class RandomNumberGenerator : ServiceComponent<IRandom>, IRandom
{
    readonly Lock _syncRoot = new();
    readonly Random _random = new();

    public int Generate(int max)
    {
        lock (_syncRoot)
            return _random.Next(max);
    }
}