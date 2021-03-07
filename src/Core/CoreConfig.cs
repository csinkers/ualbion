using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using UAlbion.Api;

namespace UAlbion.Core
{
#pragma warning disable CA1034 // Nested types should not be visible
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
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

        public static CoreConfig Load(string configPath, IFileSystem disk)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            if (!disk.FileExists(configPath))
                return new CoreConfig();

            var configText = disk.ReadAllText(configPath);
            return (CoreConfig)JsonConvert.DeserializeObject<CoreConfig>(configText,
                new JsonSerializerSettings { ContractResolver = new PrivatePropertyJsonContractResolver() });
        }
    }
#pragma warning restore CA1034 // Nested types should not be visible
}
