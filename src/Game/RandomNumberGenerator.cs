using System;
using UAlbion.Api.Eventing;
using UAlbion.Core;

namespace UAlbion.Game;

public class RandomNumberGenerator : ServiceComponent<IRandom>, IRandom
{
    readonly object _syncRoot = new();
    readonly Random _random = new();

    public int Generate(int max)
    {
        lock (_syncRoot)
            return _random.Next(max);
    }
}