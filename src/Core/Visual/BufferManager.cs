using System;

namespace UAlbion.Core.Visual
{
    public interface IDeviceObjectManager
    {
        T GetDeviceObject<T>((object, object) owner) where T : IDisposable;
        void SetDeviceObject<T>((object, object) owner, T newResource) where T : IDisposable;
        void DestroyDeviceObjects();
        string Stats();
    }
}
