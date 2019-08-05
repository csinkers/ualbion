using System.Collections.Generic;
using Veldrid;

namespace UAlbion.Core
{
    // Non-thread-safe cache for resources.
    internal static class StaticResourceCache
    {
        static readonly IDictionary<ShaderSetCacheKey, (Shader, Shader)> ShaderSets = new Dictionary<ShaderSetCacheKey, (Shader, Shader)>();

        public static (Shader vs, Shader fs) GetShaders(
            GraphicsDevice gd,
            ResourceFactory factory,
            string name)
        {
            SpecializationConstant[] constants = ShaderHelper.GetSpecializations(gd);
            ShaderSetCacheKey cacheKey = new ShaderSetCacheKey(name, constants);
            if (!ShaderSets.TryGetValue(cacheKey, out (Shader vs, Shader fs) set))
            {
                set = ShaderHelper.LoadSPIRV(gd, factory, name);
                ShaderSets.Add(cacheKey, set);
            }

            return set;
        }

        public static void DestroyAllDeviceObjects()
        {
            foreach (KeyValuePair<ShaderSetCacheKey, (Shader, Shader)> kvp in ShaderSets)
            {
                kvp.Value.Item1.Dispose();
                kvp.Value.Item2.Dispose();
            }
            ShaderSets.Clear();
        }
    }
}
