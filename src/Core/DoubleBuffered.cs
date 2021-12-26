using System;

namespace UAlbion.Core
{
    public class DoubleBuffered<T>
    {
        readonly object _syncRoot = new();

        public DoubleBuffered(Func<T> constructor)
        {
            if (constructor == null) throw new ArgumentNullException(nameof(constructor));
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
}