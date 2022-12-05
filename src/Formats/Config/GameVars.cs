using UAlbion.Api.Settings;

namespace UAlbion.Formats.Config;

public static class GameVars
{
    public static class Audio
    {
        public static readonly FloatVar PollIntervalSeconds = new("Audio.PollIntervalSeconds", 0.1f);
    }

    public static class Inventory
    {
        public static readonly IntVar CarryWeightPerStrength = new("Inventory.CarryWeightPerStrength", 1000);
        public static readonly IntVar GramsPerGold = new("Inventory.GramsPerGold", 20);
        public static readonly IntVar GramsPerRation = new("Inventory.GramsPerRation", 250);
    }

    public static class NpcMovement
    {
        public static readonly IntVar TicksPerFrame = new("NpcMovement.TicksPerFrame", 12); // Number of game ticks it takes to advance to the next animation frame
        public static readonly IntVar TicksPerTile = new("NpcMovement.TicksPerTile", 24); // Number of game ticks it takes to move across a map tile
    }
    public static class PartyMovement
    {
        public static readonly IntVar MaxTrailDistanceLarge = new("PartyMovement.MaxTrailDistanceLarge", 18);
        public static readonly IntVar MaxTrailDistanceSmall = new("PartyMovement.MaxTrailDistanceSmall", 12);
        public static readonly IntVar MinTrailDistanceLarge = new("PartyMovement.MinTrailDistanceLarge", 12);
        public static readonly IntVar MinTrailDistanceSmall = new("PartyMovement.MinTrailDistanceSmall", 6);
        public static readonly IntVar TicksPerFrame = new("PartyMovement.TicksPerFrame", 9); // Number of game ticks it takes to advance to the next animation frame
        public static readonly IntVar TicksPerTile = new("PartyMovement.TicksPerTile", 12); // Number of game ticks it takes to move across a map tile
    }
    public static class Time
    {
        public static readonly IntVar FastTicksPerAssetCacheCycle = new("Time.FastTicksPerAssetCacheCycle", 3600);
        public static readonly IntVar FastTicksPerMapTileFrame = new("Time.FastTicksPerMapTileFrame", 10);
        public static readonly FloatVar FastTicksPerSecond = new("Time.FastTicksPerSecond", 60.0f);
        public static readonly IntVar FastTicksPerSlowTick = new("Time.FastTicksPerSlowTick", 8); // Used for palette cycling & 'every-step' events
        public static readonly FloatVar GameSecondsPerSecond = new("Time.GameSecondsPerSecond", 180.0f);
        public static readonly FloatVar IdleTicksPerSecond = new("Time.IdleTicksPerSecond", 8.0f);
    }
    public static class Ui
    {
        public static readonly FloatVar ButtonDoubleClickIntervalSeconds = new("UI.ButtonDoubleClickIntervalSeconds", 0.35f);
        public static readonly FloatVar MouseLookSensitivity = new("UI.MouseLookSensitivity", 2.0f);

        public static class Transitions
        {
            public static readonly FloatVar DefaultTransitionTimeSeconds = new("UI.Transitions.DefaultTransitionTimeSeconds", 0.35f);
            public static readonly FloatVar DiscardItemGravity = new("UI.Transitions.DiscardItemGravity", 9.8f);
            public static readonly FloatVar DiscardItemMaxInitialX = new("UI.Transitions.DiscardItemMaxInitialX", 2.0f);
            public static readonly FloatVar DiscardItemMaxInitialY = new("UI.Transitions.DiscardItemMaxInitialY", 2.2f);
            public static readonly FloatVar InventoryChangLerpSeconds = new("UI.Transitions.InventoryChangLerpSeconds", 0.25f);
            public static readonly FloatVar ItemMovementTransitionTimeSeconds = new("UI.Transitions.ItemMovementTransitionTimeSeconds", 0.2f);
            public static readonly IntVar MaxDiscardTransitions = new("UI.Transitions.MaxDiscardTransitions", 24);
        }
    }
    public static class Visual
    {
        public static class Camera2D
        {
            public static readonly FloatVar LerpRate = new("Visual.Camera2D.LerpRate", 3.0f);
            public static readonly FloatVar TileOffsetX = new("Visual.Camera2D.TileOffsetX", 0.0f);
            public static readonly FloatVar TileOffsetY = new("Visual.Camera2D.TileOffsetY", 0.0f);
        }
    }
}