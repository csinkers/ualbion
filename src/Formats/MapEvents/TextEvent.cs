using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("text")] // USED IN SCRIPT
public class TextEvent : MapEvent // Relies on event chain context to resolve TextId to an enum type / AssetId
{
    TextEvent() { }
    public TextEvent(ushort subId, TextLocation location, SheetId sheetId)
    {
        SubId = subId;
        Location = location;

        var expectedType = AssetTypeForTextLocation(location);
        if (sheetId.Type != expectedType && !sheetId.IsNone)
        {
            throw new FormatException(
                "Tried to construct a text event with location " +
                $"{location} and a character id of type {sheetId.Type}, but a {expectedType} was expected");
        }

        Speaker = sheetId;
    }

    [EventPart("id")] public ushort SubId { get; private set; }
    [EventPart("location", true, TextLocation.NoPortrait)] public TextLocation Location { get; private set; }
    [EventPart("speaker", true, "None")] public SheetId Speaker { get; private set; }
    public override MapEventType EventType => MapEventType.Text;
    public StringId ToId(StringSetId textId) => new(textId, SubId);

    public static TextEvent Parse(string[] parts)
    {
        ArgumentNullException.ThrowIfNull(parts);

        var subId = ushort.Parse(parts[1]);
        var location = parts.Length > 2 ? (TextLocation)Enum.Parse(typeof(TextLocation), parts[2]) : TextLocation.NoPortrait;

        if (parts.Length <= 3)
            return new TextEvent(subId, location, SheetId.None);

        if (int.TryParse(parts[3], out var id))
        {
            if (id <= 0)
                return new TextEvent(subId, location, SheetId.None);

            var type = AssetTypeForTextLocation(location);
            return new TextEvent(subId, location, new SheetId(type, id));
        }

        var charId = SheetId.Parse(parts[3]);
        return new TextEvent(subId, location, charId);
    }

    public static TextEvent Serdes(TextEvent e, AssetMapping mapping, ISerializer s)
    {
        e ??= new TextEvent();

        ArgumentNullException.ThrowIfNull(s);
        e.Location = s.EnumU8(nameof(Location), e.Location); // 1
        int zeroed = s.UInt16(null, 0); // 2, 3

        e.Speaker = SheetId.SerdesU8( // 4
            nameof(Speaker),
            e.Speaker,
            AssetTypeForTextLocation(e.Location),
            mapping,
            s);

        e.SubId = s.UInt16(nameof(SubId), e.SubId);
        zeroed += s.UInt8(null, 0);
        zeroed += s.UInt16(null, 0);
        s.Assert(zeroed == 0, "TextEvent: Expected fields 2,3,6,8 to be 0");
        return e;
    }

    static AssetType AssetTypeForTextLocation(TextLocation location) =>
        location switch
        {
            // TODO: Handle the other cases
            TextLocation.PortraitLeft => AssetType.PartySheet,
            _ => AssetType.NpcSheet
        };
}
