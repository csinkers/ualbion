using System;
using System.Collections.Generic;

namespace UAlbion.Core
{
    public class Pooled<T> where T : class
    {
        readonly Func<T> _constructor;
        readonly Action<T> _cleanFunc;
        readonly Stack<T> _free = new();

        public Pooled(Func<T> constructor, Action<T> cleanFunc)
        {
            _constructor = constructor ?? throw new ArgumentNullException(nameof(constructor));
            _cleanFunc = cleanFunc;
        }

        public T Borrow()
        {
            if (_free.TryPop(out var result))
                return result;

            return _constructor();
        }

        public void Return(T instance)
        {
            _cleanFunc?.Invoke(instance);
            _free.Push(instance);
        }
    }
}