﻿using System;
using System.Threading;

namespace UAlbion.Api;

public class DoubleBuffered<T>
{
    readonly Lock _syncRoot = new();

    public DoubleBuffered(Func<T> constructor)
    {
        ArgumentNullException.ThrowIfNull(constructor);
        Front = constructor();
        Back = constructor();
    }

    public T Front { get; private set; }

    public T Back { get; private set; }

    public void Swap()
    {
        lock (_syncRoot)
        {
            (Front, Back) = (Back, Front);
        }
    }
}
