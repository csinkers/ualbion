using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using UAlbion.Api;

#pragma warning disable CA1034 // Nested types should not be visible
namespace UAlbion.Formats.Config;

public class GameConfig
{
    [JsonInclude] public VisualT Visual { get; private set; } = new();
    public class VisualT
    {
        [JsonInclude] public TextureManagerT TextureManager { get; private set; } = new();
        public class TextureManagerT
        {
            [JsonInclude] public float CacheLifetimeSeconds { get; private set; }
            [JsonInclude] public float CacheCheckIntervalSeconds { get; private set; }
        }

        [JsonInclude] public Camera2DT Camera2D { get; private set; } = new();
        public class Camera2DT
        {
            [JsonInclude] public float LerpRate { get; private set; }
        }

        [JsonInclude] public SkyboxT Skybox { get; private set; } = new();
        public class SkyboxT
        {
            [JsonInclude] public float VisibleProportion { get; private set; }
        }
    }

    [JsonInclude] public TimeT Time { get; private set; } = new();
    public class TimeT
    {
        [JsonInclude] public float FastTicksPerSecond { get; private set; }
        [JsonInclude] public float GameSecondsPerSecond { get; private set; }
        [JsonInclude] public int FastTicksPerAssetCacheCycle { get; private set; }
        [JsonInclude] public int IdleTicksPerSecond { get; private set; }
        [JsonInclude] public int FastTicksPerSlowTick { get; private set; }
        [JsonInclude] public int FastTicksPerMapTileFrame { get; private set; }
    }

    [JsonInclude] public MovementT PartyMovement { get; private set; } = new();
    [JsonInclude] public MovementT NpcMovement { get; private set; } = new();
    public class MovementT
    {
        [JsonInclude] public int TicksPerTile { get; private set; } // Number of game ticks it takes to move across a map tile
        [JsonInclude] public int TicksPerFrame { get; private set; } // Number of game ticks it takes to advance to the next animation frame
        [JsonInclude] public int MinTrailDistanceSmall { get; private set; }
        [JsonInclude] public int MaxTrailDistanceSmall { get; private set; } // Max number of positions between each character in the party. Looks best if coprime to TicksPerPile and TicksPerFrame.
        [JsonInclude] public int MinTrailDistanceLarge { get; private set; }
        [JsonInclude] public int MaxTrailDistanceLarge { get; private set; } // Max number of positions between each character in the party. Looks best if coprime to TicksPerPile and TicksPerFrame.
    }

    [JsonInclude] public UIT UI { get; private set; } = new();
    public class UIT
    {
        [JsonInclude] public TransitionsT Transitions { get; private set; } = new();
        public class TransitionsT
        {
            [JsonInclude] public float DiscardItemGravity { get; private set; }
            [JsonInclude] public float DiscardItemMaxInitialX { get; private set; }
            [JsonInclude] public float DiscardItemMaxInitialY { get; private set; }
            [JsonInclude] public int MaxDiscardTransitions { get; private set; }
            [JsonInclude] public float DefaultTransitionTimeSeconds { get; private set; }
            [JsonInclude] public float ItemMovementTransitionTimeSeconds { get; private set; }
            [JsonInclude] public float InventoryChangLerpSeconds { get; private set; }
        }

        [JsonInclude] public float ButtonDoubleClickIntervalSeconds { get; private set; }
        [JsonInclude] public float MouseLookSensitivity { get; private set; }
    }

    [JsonInclude] public AudioT Audio { get; private set; } = new();
    public class AudioT
    {
        [JsonInclude] public float AudioPollIntervalSeconds { get; private set; }
    }

    [JsonInclude] public InventoryT Inventory { get; private set; } = new();
    public class InventoryT
    {
        [JsonInclude]
        public IDictionary<string, InventoryPositionDictionary> Positions { get; private set; }
            = new Dictionary<string, InventoryPositionDictionary>();

        [JsonInclude] public int GramsPerGold { get; private set; }
        [JsonInclude] public int GramsPerRation { get; private set; }
        [JsonInclude] public int CarryWeightPerStrength { get; private set; }
    }

    public static GameConfig Load(string configPath, IFileSystem disk, IJsonUtil jsonUtil)
    {
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));
        if (!disk.FileExists(configPath))
            throw new FileNotFoundException($"Could not find game config file at expected path {configPath}");

        var configText = disk.ReadAllBytes(configPath);
        return jsonUtil.Deserialize<GameConfig>(configText);
    }

    public static GameConfig LoadLiteral(byte[] json, IJsonUtil jsonUtil)
    {
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));
        return jsonUtil.Deserialize<GameConfig>(json);
    }
}
#pragma warning restore CA1034 // Nested types should not be visible
