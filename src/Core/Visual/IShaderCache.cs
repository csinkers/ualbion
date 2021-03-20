using System;

namespace UAlbion.Core.Visual
{
    public interface IShaderCache : IComponent
    {
        event EventHandler<EventArgs> ShadersUpdated;
        string GetGlsl(string shaderName);
        void CleanupOldFiles();
        void DestroyAllDeviceObjects();
        IShaderCache AddShaderPath(string path);
    }
}