using UAlbion.Core;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public class Settings : Component, ISettings
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
            H<Settings, SetMusicVolumeEvent>((x, e)       => x.MusicVolume       = e.Value),
            H<Settings, SetFxVolumeEvent>((x, e)          => x.FxVolume          = e.Value),
            H<Settings, SetWindowSize3dEvent>((x, e)      => x.WindowSize3d      = e.Value),
            H<Settings, SetCombatDetailLevelEvent>((x, e) => x.CombatDetailLevel = e.Value),
            H<Settings, SetCombatDelayEvent>((x, e)       => x.CombatDelay       = e.Value)
        );

        public Settings() : base(Handlers) { }
        public GameLanguage Language { get; private set; }
        public int MusicVolume { get; private set; } = 32;
        public int FxVolume { get; private set; } = 48;
        public int WindowSize3d { get; private set; } = 15;
        public int CombatDetailLevel { get; private set; } = 1;
        public int CombatDelay { get; private set; } = 3;
    }
}
