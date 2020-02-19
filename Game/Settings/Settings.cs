using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats;
using UAlbion.Game.Events;

namespace UAlbion.Game.Settings
{
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
            H<Settings, DebugFlagEvent>    ((x, e) => x.DebugFlags = (DebugFlags)CoreUtil.UpdateFlag((uint)x.DebugFlags, e.Operation, (uint)e.Flag)),
            H<Settings, SpecialEvent>      ((x, e) => x.Special1 = CoreUtil.UpdateValue(x.Special1, e.Operation, e.Argument)),
            H<Settings, Special2Event>     ((x, e) => x.Special2 = CoreUtil.UpdateValue(x.Special2, e.Operation, e.Argument)),
            H<Settings, EngineFlagEvent>   ((x, e) => x.Flags = (EngineFlags)CoreUtil.UpdateFlag((uint)x.Flags, e.Operation, (uint)e.Flag))
        );

        public Settings() : base(Handlers) { }
        public string BasePath { get; set; }

        IDebugSettings ISettings.Debug => this;
        IAudioSettings ISettings.Audio => this;
        IGraphicsSettings ISettings.Graphics => this;
        IGameplaySettings ISettings.Gameplay => this;
        IEngineSettings ISettings.Engine => this;

        // Debug
        public DebugFlags DebugFlags { get; private set; }

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
