using System;
using System.Diagnostics.CodeAnalysis;
using UAlbion.Api;

namespace UAlbion.Core;
#pragma warning disable CA1034 // Nested types should not be visible

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
public class CoreConfig
{
    public VisualT Visual { get; } = new();
    public class VisualT
    {
        public TextureManagerT TextureManager { get; } = new();
        public class TextureManagerT
        {
            public float CacheCheckIntervalSeconds { get; private set; } = 5.0f;
        }

        public SpriteManagerT SpriteManager { get; } = new();
        public class SpriteManagerT
        {
            public float CacheCheckIntervalSeconds { get; private set; } = 12.0f;
        }

        public SkyboxT Skybox { get; } = new();
        public class SkyboxT
        {
            public float VisibleProportion { get; private set; }
        }
    }

    public static CoreConfig Load(string configPath, IFileSystem disk, IJsonUtil jsonUtil)
    {
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));
        if (!disk.FileExists(configPath))
            return new CoreConfig();

        var configText = disk.ReadAllBytes(configPath);
        return jsonUtil.Deserialize<CoreConfig>(configText);
    }
}
#pragma warning restore CA1034 // Nested types should not be visible