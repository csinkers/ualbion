using System;

namespace UAlbion.Core.Visual
{
    public interface IDeviceObjectManager
    {
        T Get<T>((object, object) owner) where T : IDisposable;
        void Set<T>((object, object) owner, T newResource) where T : IDisposable;
        void DestroyDeviceObjects();
        string Stats();
    }
}
