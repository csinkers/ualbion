using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("set_language", "Change the current game language.")]
    public class SetLanguageEvent : GameEvent
    {
        public SetLanguageEvent(GameLanguage language) { Language = language; }
        [EventPart("language")] public GameLanguage Language { get; }
    }
}