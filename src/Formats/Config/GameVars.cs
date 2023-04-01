using UAlbion.Api.Settings;

namespace UAlbion.Formats.Config;

public static class GameVars
{
    public static class Audio
    {
        public static readonly FloatVar PollIntervalSeconds = new("Game.Audio.PollIntervalSeconds", 0.1f);
    }

    public static class Inventory
    {
        public static readonly IntVar CarryWeightPerStrength = new("Game.Inventory.CarryWeightPerStrength", 1000);
        public static readonly IntVar GramsPerGold           = new("Game.Inventory.GramsPerGold", 20);
        public static readonly IntVar GramsPerRation         = new("Game.Inventory.GramsPerRation", 250);
    }

    public static class NpcMovement
    {
        public static readonly IntVar TicksPerFrame = new("Game.NpcMovement.TicksPerFrame", 12); // Number of game ticks it takes to advance to the next animation frame
        public static readonly IntVar TicksPerTile  = new("Game.NpcMovement.TicksPerTile", 24); // Number of game ticks it takes to move across a map tile
    }

    public static class PartyMovement
    {
        public static readonly IntVar MaxTrailDistanceLarge = new("Game.PartyMovement.MaxTrailDistanceLarge", 18);
        public static readonly IntVar MaxTrailDistanceSmall = new("Game.PartyMovement.MaxTrailDistanceSmall", 12);
        public static readonly IntVar MinTrailDistanceLarge = new("Game.PartyMovement.MinTrailDistanceLarge", 12);
        public static readonly IntVar MinTrailDistanceSmall = new("Game.PartyMovement.MinTrailDistanceSmall", 6);
        public static readonly IntVar TicksPerFrame         = new("Game.PartyMovement.TicksPerFrame", 9); // Number of game ticks it takes to advance to the next animation frame
        public static readonly IntVar TicksPerTile          = new("Game.PartyMovement.TicksPerTile", 12); // Number of game ticks it takes to move across a map tile
    }

    public static class Time
    {
        public static readonly IntVar FastTicksPerAssetCacheCycle = new("Game.Time.FastTicksPerAssetCacheCycle", 3600);
        public static readonly IntVar FastTicksPerMapTileFrame    = new("Game.Time.FastTicksPerMapTileFrame", 10);
        public static readonly FloatVar FastTicksPerSecond        = new("Game.Time.FastTicksPerSecond", 60.0f);
        public static readonly IntVar FastTicksPerSlowTick        = new("Game.Time.FastTicksPerSlowTick", 8); // Used for palette cycling & 'every-step' events
        public static readonly FloatVar GameSecondsPerSecond      = new("Game.Time.GameSecondsPerSecond", 180.0f);
        public static readonly FloatVar IdleTicksPerSecond        = new("Game.Time.IdleTicksPerSecond", 8.0f);
    }

    public static class Ui
    {
        public static readonly FloatVar ButtonDoubleClickIntervalSeconds = new("Game.UI.ButtonDoubleClickIntervalSeconds", 0.35f);
        public static readonly FloatVar MouseLookSensitivity             = new("Game.UI.MouseLookSensitivity", 2.0f);

        public static class Transitions
        {
            public static readonly FloatVar DefaultTransitionTimeSeconds      = new("Game.UI.Transitions.DefaultTransitionTimeSeconds", 0.35f);
            public static readonly FloatVar DiscardItemGravity                = new("Game.UI.Transitions.DiscardItemGravity", 9.8f);
            public static readonly FloatVar DiscardItemMaxInitialX            = new("Game.UI.Transitions.DiscardItemMaxInitialX", 2.0f);
            public static readonly FloatVar DiscardItemMaxInitialY            = new("Game.UI.Transitions.DiscardItemMaxInitialY", 2.2f);
            public static readonly FloatVar InventoryChangLerpSeconds         = new("Game.UI.Transitions.InventoryChangLerpSeconds", 0.25f);
            public static readonly FloatVar ItemMovementTransitionTimeSeconds = new("Game.UI.Transitions.ItemMovementTransitionTimeSeconds", 0.2f);
            public static readonly IntVar   MaxDiscardTransitions             = new("Game.UI.Transitions.MaxDiscardTransitions", 24);
        }
    }

    public static class VisualVars // added Vars suffix to avoid clashing with UAlbion.Api.Visual namespace etc
    {
        public static class Camera2D
        {
            public static readonly FloatVar LerpRate = new("Game.Visual.Camera2D.LerpRate", 3.0f);
            public static readonly FloatVar TileOffsetX = new("Game.Visual.Camera2D.TileOffsetX", 0.0f);
            public static readonly FloatVar TileOffsetY = new("Game.Visual.Camera2D.TileOffsetY", 0.0f);
        }
    }
}