using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using UAlbion.Api;

namespace UAlbion.Core;
#pragma warning disable CA1034 // Nested types should not be visible

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
public class CoreConfig
{
    [JsonInclude] public VisualT Visual { get; private set; } = new();
    public class VisualT
    {
        [JsonInclude] public TextureManagerT TextureManager { get; private set; } = new();
        public class TextureManagerT
        {
            [JsonInclude] public float CacheCheckIntervalSeconds { get; private set; } = 5.0f;
        }

        [JsonInclude] public SpriteManagerT SpriteManager { get; private set; } = new();
        public class SpriteManagerT
        {
            [JsonInclude] public float CacheCheckIntervalSeconds { get; private set; } = 12.0f;
        }

        [JsonInclude] public SkyboxT Skybox { get; private set; } = new();
        public class SkyboxT
        {
            [JsonInclude] public float VisibleProportion { get; private set; }
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