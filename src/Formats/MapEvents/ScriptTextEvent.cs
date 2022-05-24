using System;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("text")] // USED IN SCRIPT
public class ScriptTextEvent : Event, IAsyncEvent // Relies on event chain context to resolve TextId to an enum type / AssetId
{
    public ScriptTextEvent(ushort textId, TextLocation location, SheetId SheetId)
    {
        TextId = textId;
        Location = location;

        var expectedType = MapTextEvent.AssetTypeForTextLocation(location);
        if (SheetId.Type != expectedType && !SheetId.IsNone)
        {
            throw new FormatException(
                "Tried to construct a text event with location " +
                $"{location} and a character id of type {SheetId.Type}, but a {expectedType} was expected");
        }

        Speaker = SheetId;
    }

    [EventPart("text_id")] public ushort TextId { get; }
    [EventPart("location", true, TextLocation.NoPortrait)] public TextLocation Location { get; }
    [EventPart("speaker", true, "None")] public SheetId Speaker { get; }

    public static ScriptTextEvent Parse(string[] parts)
    {
        var textId = ushort.Parse(parts[1]);
        var location = parts.Length > 2 ? (TextLocation)Enum.Parse(typeof(TextLocation), parts[2]) : TextLocation.NoPortrait;

        if (parts.Length <= 3)
            return new ScriptTextEvent(textId, location, SheetId.None);

        if (int.TryParse(parts[3], out var id))
        {
            if (id <= 0)
                return new ScriptTextEvent(textId, location, SheetId.None);

            var type = MapTextEvent.AssetTypeForTextLocation(location);
            return new ScriptTextEvent(textId, location, new SheetId(type, id));
        }

        var charId = SheetId.Parse(parts[3]);
        return new ScriptTextEvent(textId, location, charId);
    }
}
