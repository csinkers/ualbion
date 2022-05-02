using System;
using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("set_language", "Change the current game language.")]
public class SetLanguageEvent : GameEvent
{
    public SetLanguageEvent(string language) => Language = (language ?? throw new ArgumentNullException(nameof(language))).ToUpperInvariant();
    [EventPart("language")] public string Language { get; }
}

// Raised after the language has changed to prompt text fields etc to reload their contents