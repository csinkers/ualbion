using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Newtonsoft.Json;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

#pragma warning disable CA1034 // Nested types should not be visible
namespace UAlbion.Formats.Config
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    public class GameConfig
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

            public Camera2DT Camera2D { get; } = new Camera2DT();
            public class Camera2DT
            {
                public float LerpRate { get; private set; }
            }

            public SkyboxT Skybox { get; } = new SkyboxT();
            public class SkyboxT
            {
                public float VisibleProportion { get; private set; }
            }
        }

        public TimeT Time { get; } = new TimeT();
        public class TimeT
        {
            public float FastTicksPerSecond { get; private set; }
            public float GameSecondsPerSecond { get; private set; }
            public int FastTicksPerAssetCacheCycle { get; private set; }
            public int IdleTicksPerSecond { get; private set; }
            public int FastTicksPerSlowTick { get; private set; }
            public int FastTicksPerMapTileFrame { get; private set; }
        }

        public UIT UI { get; } = new UIT();
        public class UIT
        {
            public TransitionsT Transitions { get; } = new TransitionsT();
            public class TransitionsT
            {
                public float DiscardItemGravity { get; private set; }
                public float DiscardItemMaxInitialX { get; private set; }
                public float DiscardItemMaxInitialY { get; private set; }
                public int MaxDiscardTransitions { get; private set; }
                public float DefaultTransitionTimeSeconds { get; private set; }
                public float ItemMovementTransitionTimeSeconds { get; private set; }
                public float InventoryChangLerpSeconds { get; private set; }
            }

            public float ButtonDoubleClickIntervalSeconds { get; private set; }
            public float MouseLookSensitivity { get; private set; }
        }

        public AudioT Audio { get; } = new AudioT();
        public class AudioT
        {
            public float AudioPollIntervalSeconds { get; private set; }
        }

        public InventoryT Inventory { get; } = new InventoryT();
        public class InventoryT
        {
            public IDictionary<PartyCharacterId, InventoryPositionDictionary> Positions { get; }
                = new Dictionary<PartyCharacterId, InventoryPositionDictionary>();

            public int GramsPerGold { get; private set; }
            public int GramsPerRation { get; private set; }
            public int CarryWeightPerStrength { get; private set; }
        }

        public static GameConfig Load(string basePath)
        {
            var configPath = Path.Combine(basePath, "data", "game.json");
            if (!File.Exists(configPath))
                return new GameConfig();

            var configText = File.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<GameConfig>(configText,
                new JsonSerializerSettings {ContractResolver = new PrivatePropertyJsonContractResolver()});
        }

        public static GameConfig LoadLiteral(string json) =>
            JsonConvert.DeserializeObject<GameConfig>(json,
                new JsonSerializerSettings {ContractResolver = new PrivatePropertyJsonContractResolver()});
    }
}
#pragma warning restore CA1034 // Nested types should not be visible
