using System;

namespace UAlbion.Core.Visual
{
    public interface IDeviceObjectManager
    {
        T Get<T>((object, object) owner);
        T Prepare<T>((object, object) owner, Func<T> createFunc, Func<T, bool> dirtyFunc) where T : IDisposable;
        void DestroyDeviceObjects();
        string Stats();
    }
}
