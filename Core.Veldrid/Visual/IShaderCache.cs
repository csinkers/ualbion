using System;
using Veldrid;

namespace UAlbion.Core.Veldrid.Visual
{
    public interface IShaderCache
    {
        event EventHandler<EventArgs> ShadersUpdated;
        string GetGlsl(string shaderName);
        Shader[] GetShaderPair(ResourceFactory factory,
            string vertexShaderName, string fragmentShaderName,
            string vertexShaderContent, string fragmentShaderContent);

        void CleanupOldFiles();
        void DestroyAllDeviceObjects();

    }
}