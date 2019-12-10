using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Game.Events;

namespace UAlbion.Game.Settings
{
    [Event("draw_positions")] public class SetDrawPositionsEvent : GameEvent {[EventPart("value")] public bool Value { get; } public SetDrawPositionsEvent(bool value) { Value = value; } }
    [Event("highlight_tile")] public class SetHighlightTileEvent : GameEvent {[EventPart("value")] public bool Value { get; } public SetHighlightTileEvent(bool value) { Value = value; } }
    [Event("highlight_selection")] public class SetHighlightSelectionEvent : GameEvent {[EventPart("value")] public bool Value { get; } public SetHighlightSelectionEvent(bool value) { Value = value; } }
    [Event("highlight_zones")] public class SetHighlightEventChainZonesEvent : GameEvent {[EventPart("value")] public bool Value { get; } public SetHighlightEventChainZonesEvent(bool value) { Value = value; } }
    [Event("show_paths")] public class SetShowPathsEvent : GameEvent {[EventPart("value")] public bool Value { get; } public SetShowPathsEvent(bool value) { Value = value; } }

    public class Settings : Component, ISettings
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<Settings, SetLanguageEvent>((x, e) =>
            {
                if (x.Gameplay.Language != e.Language)
                {
                    x.Gameplay.Language = e.Language;
                    x.Raise(e); // Re-raise to ensure any consumers who received it before Settings will get it again.
                }
            }),
            H<Settings, SetMusicVolumeEvent>((x, e)       => x.Audio.MusicVolume = e.Value),
            H<Settings, SetFxVolumeEvent>((x, e)          => x.Audio.FxVolume = e.Value),
            H<Settings, SetWindowSize3dEvent>((x, e)      => x.Graphics.WindowSize3d = e.Value),
            H<Settings, SetCombatDetailLevelEvent>((x, e) => x.Graphics.CombatDetailLevel = e.Value),
            H<Settings, SetCombatDelayEvent>((x, e)       => x.Gameplay.CombatDelay = e.Value),
            H<Settings, SetDrawPositionsEvent>((x, e)     => x.Debug.DrawPositions = e.Value),
            H<Settings, SetHighlightTileEvent>((x, e)     => x.Debug.HighlightTile = e.Value),
            H<Settings, SetHighlightSelectionEvent>((x, e) => x.Debug.HighlightSelection = e.Value),
            H<Settings, SetHighlightEventChainZonesEvent>((x, e) => x.Debug.HighlightEventChainZones = e.Value),
            H<Settings, SetShowPathsEvent>((x, e)         => x.Debug.ShowPaths = e.Value)
        );

        public Settings() : base(Handlers) { }
        public string BasePath { get; set; }

        readonly DebugSettings Debug = new DebugSettings();
        readonly AudioSettings Audio = new AudioSettings();
        readonly GraphicsSettings Graphics = new GraphicsSettings();
        readonly GameplaySettings Gameplay = new GameplaySettings();

        IDebugSettings ISettings.Debug => Debug;
        IAudioSettings ISettings.Audio => Audio;
        IGraphicsSettings ISettings.Graphics => Graphics;
        IGameplaySettings ISettings.Gameplay => Gameplay;

        class DebugSettings : IDebugSettings
        {
            public bool DrawPositions { get; set; }
            public bool HighlightTile { get; set; }
            public bool HighlightSelection { get; set; }
            public bool HighlightEventChainZones { get; set; }
            public bool ShowPaths { get; set; }
        }

        class AudioSettings : IAudioSettings
        {
            public int MusicVolume { get; set; } = 32;
            public int FxVolume { get; set; } = 48;
        }

        class GraphicsSettings : IGraphicsSettings
        {
            public int WindowSize3d { get; set; } = 15;
            public int CombatDetailLevel { get; set; } = 1;
        }

        class GameplaySettings : IGameplaySettings
        {
            public GameLanguage Language { get; set; }
            public int CombatDelay { get; set; } = 3;
        }
    }
}
