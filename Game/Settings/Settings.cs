using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats;
using UAlbion.Game.Events;
using Util = UAlbion.Core.Util;

namespace UAlbion.Game.Settings
{
    [Event("draw_positions")] public class SetDrawPositionsEvent : GameEvent {[EventPart("value")] public bool Value { get; } public SetDrawPositionsEvent(bool value) { Value = value; } }
    [Event("highlight_tile")] public class SetHighlightTileEvent : GameEvent {[EventPart("value")] public bool Value { get; } public SetHighlightTileEvent(bool value) { Value = value; } }
    [Event("highlight_selection")] public class SetHighlightSelectionEvent : GameEvent {[EventPart("value")] public bool Value { get; } public SetHighlightSelectionEvent(bool value) { Value = value; } }
    [Event("highlight_zones")] public class SetHighlightEventChainZonesEvent : GameEvent {[EventPart("value")] public bool Value { get; } public SetHighlightEventChainZonesEvent(bool value) { Value = value; } }
    [Event("show_paths")] public class SetShowPathsEvent : GameEvent {[EventPart("value")] public bool Value { get; } public SetShowPathsEvent(bool value) { Value = value; } }

    public class Settings : Component, ISettings, IDebugSettings, IAudioSettings, IGraphicsSettings, IGameplaySettings, IEngineSettings
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<Settings, SetLanguageEvent>((x, e) =>
            {
                if (x.Language != e.Language)
                {
                    x.Language = e.Language;
                    x.Raise(e); // Re-raise to ensure any consumers who received it before Settings will get it again.
                }
            }),
            H<Settings, SetMusicVolumeEvent>       ((x, e) => x.MusicVolume        = e.Value),
            H<Settings, SetFxVolumeEvent>          ((x, e) => x.FxVolume           = e.Value),
            H<Settings, SetWindowSize3dEvent>      ((x, e) => x.WindowSize3d       = e.Value),
            H<Settings, SetCombatDetailLevelEvent> ((x, e) => x.CombatDetailLevel  = e.Value),
            H<Settings, SetCombatDelayEvent>       ((x, e) => x.CombatDelay        = e.Value),
            H<Settings, SetDrawPositionsEvent>     ((x, e) => x.DrawPositions      = e.Value),
            H<Settings, SetHighlightTileEvent>     ((x, e) => x.HighlightTile      = e.Value),
            H<Settings, SetHighlightSelectionEvent>((x, e) => x.HighlightSelection = e.Value),
            H<Settings, SetHighlightEventChainZonesEvent>((x, e) => x.HighlightEventChainZones = e.Value),
            H<Settings, SetShowPathsEvent> ((x, e) => x.ShowPaths = e.Value),
            H<Settings, SpecialEvent>      ((x, e) => x.Special1 = Util.UpdateValue(x.Special1, e.Operation, e.Argument)),
            H<Settings, Special2Event>     ((x, e) => x.Special2 = Util.UpdateValue(x.Special2, e.Operation, e.Argument)),
            H<Settings, EngineFlagEvent>   ((x, e) => x.Flags = (EngineFlags)Util.UpdateFlag((uint)x.Flags, e.Operation, (uint)e.Flag))
        );

        public Settings() : base(Handlers) { }
        public string BasePath { get; set; }

        IDebugSettings ISettings.Debug => this;
        IAudioSettings ISettings.Audio => this;
        IGraphicsSettings ISettings.Graphics => this;
        IGameplaySettings ISettings.Gameplay => this;
        IEngineSettings ISettings.Engine => this;

        // Debug
        public bool DrawPositions { get; private set; }
        public bool HighlightTile { get; private set; }
        public bool HighlightSelection { get; private set; }
        public bool HighlightEventChainZones { get; private set; }
        public bool ShowPaths { get; private set; }

        // Audio
        public int MusicVolume { get; private set; } = 32;
        public int FxVolume { get; private set; } = 48;

        // Graphics
        public int WindowSize3d { get; private set; } = 15;
        public int CombatDetailLevel { get; private set; } = 1;

        // Gameplay
        public GameLanguage Language { get; private set; } = GameLanguage.English;
        public int CombatDelay { get; private set; } = 3;

        // Engine
        public float Special1 { get; private set; }
        public float Special2 { get; private set; }
        public EngineFlags Flags { get; private set; } = EngineFlags.VSync;
    }
}
