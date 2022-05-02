using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents;

[Event("map_text")]
public class MapTextEvent : MapEvent, ITextEvent, IAsyncEvent
{
    MapTextEvent(TextId textSourceId) => TextSource = textSourceId;
    public MapTextEvent(TextId textSourceId, ushort subId, TextLocation location, CharacterId speaker)
    {
        TextSource = textSourceId;
        SubId = subId;
        Location = location;
        Speaker = speaker;
    }

    public static MapTextEvent Serdes(MapTextEvent e, AssetMapping mapping, ISerializer s, TextId textSourceId)
    {
        e ??= new MapTextEvent(textSourceId);
        if (e.TextSource != textSourceId)
            throw new InvalidOperationException($"Called Serdes on a TextEvent with source id {e.TextSource} but passed in source id {textSourceId}");

        if (s == null) throw new ArgumentNullException(nameof(s));
        e.TextSource = textSourceId;
        e.Location = s.EnumU8(nameof(Location), e.Location); // 1
        int zeroed = s.UInt16(null, 0); // 2, 3

        e.Speaker = CharacterId.SerdesU8( // 4
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

    [EventPart("text")] public TextId TextSource { get; private set; }
    [EventPart("sub_id")] public ushort SubId { get; private set; }
    [EventPart("location", true, TextLocation.NoPortrait)] public TextLocation Location { get; private set; }
    [EventPart("speaker", true, "None")] public CharacterId Speaker { get; private set; }

    public override MapEventType EventType => MapEventType.Text;
    public StringId ToId() => new(TextSource, SubId);

    public static AssetType AssetTypeForTextLocation(TextLocation location) =>
        location switch
        {
            // TODO: Handle the other cases
            TextLocation.PortraitLeft => AssetType.Party,
            _ => AssetType.Npc
        };
}