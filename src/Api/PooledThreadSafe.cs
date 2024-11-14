using System;
using System.Collections.Generic;
using System.Threading;

namespace UAlbion.Api;

public class PooledThreadSafe<T> where T : class
{
    readonly Lock _syncRoot = new();
    readonly Func<T> _constructor;
    readonly Action<T> _cleanFunc;
    readonly Stack<T> _free = new();

    public PooledThreadSafe(Func<T> constructor, Action<T> cleanFunc)
    {
        _constructor = constructor ?? throw new ArgumentNullException(nameof(constructor));
        _cleanFunc = cleanFunc;
    }

    public T Borrow()
    {
        lock (_syncRoot)
        {
            if (_free.TryPop(out var result))
                return result;

            return _constructor();
        }
    }

    public void Return(T instance)
    {
        _cleanFunc?.Invoke(instance);
        lock (_syncRoot)
        {
            _free.Push(instance);
        }
    }
}