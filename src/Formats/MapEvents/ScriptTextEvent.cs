using System;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents;

[Event("text")] // USED IN SCRIPT
public class ScriptTextEvent : Event, IAsyncEvent // Relies on event chain context to resolve TextId to an enum type / AssetId
{
    public ScriptTextEvent(ushort textId, TextLocation location, CharacterId characterId)
    {
        TextId = textId;
        Location = location;

        var expectedType = MapTextEvent.AssetTypeForTextLocation(location);
        if (characterId.Type != expectedType && !characterId.IsNone)
        {
            throw new FormatException(
                "Tried to construct a text event with location " +
                $"{location} and a character id of type {characterId.Type}, but a {expectedType} was expected");
        }

        Speaker = characterId;
    }

    [EventPart("text_id")] public ushort TextId { get; }
    [EventPart("location", true, TextLocation.NoPortrait)] public TextLocation Location { get; }
    [EventPart("speaker", true, "None")] public CharacterId Speaker { get; }

    public static ScriptTextEvent Parse(string[] parts)
    {
        var textId = ushort.Parse(parts[1]);
        var location = parts.Length > 2 ? (TextLocation)Enum.Parse(typeof(TextLocation), parts[2]) : TextLocation.NoPortrait;

        if (parts.Length <= 3)
            return new ScriptTextEvent(textId, location, CharacterId.None);

        if (int.TryParse(parts[3], out var id))
        {
            if (id <= 0)
                return new ScriptTextEvent(textId, location, CharacterId.None);

            var type = MapTextEvent.AssetTypeForTextLocation(location);
            return new ScriptTextEvent(textId, location, new CharacterId(type, id));
        }

        var charId = CharacterId.Parse(parts[3]);
        return new ScriptTextEvent(textId, location, charId);
    }
}