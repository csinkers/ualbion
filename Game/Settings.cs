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
            })
        );
        public Settings() : base(Handlers) { }
        public GameLanguage Language { get; private set; }
    }
}