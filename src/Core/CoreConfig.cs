using System.Diagnostics.CodeAnalysis;
using System.IO;
using Newtonsoft.Json;
using UAlbion.Api;

namespace UAlbion.Core
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
#pragma warning disable CA1034 // Nested types should not be visible
    public class CoreConfig
    {
        public VisualT Visual { get; } = new VisualT();
        public class VisualT
        {
            public TextureManagerT TextureManager { get; } = new TextureManagerT();
            public class TextureManagerT
            {
                public float CacheLifetimeSeconds { get; private set; }
                public float CacheCheckIntervalSeconds { get; private set; }
            }

            public SkyboxT Skybox { get; } = new SkyboxT();
            public class SkyboxT
            {
                public float VisibleProportion { get; private set; }
            }
        }

        public static CoreConfig Load(string basePath)
        {
            var configPath = Path.Combine(basePath, "data", "core.json");
            if (!File.Exists(configPath))
                return new CoreConfig();

            var configText = File.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<CoreConfig>(configText,
                new JsonSerializerSettings {ContractResolver = new PrivatePropertyJsonContractResolver()});
        }
    }
#pragma warning restore CA1034 // Nested types should not be visible
}