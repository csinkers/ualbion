using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    [Event("map_text")]
    public class TextEvent : MapEvent, ITextEvent, IAsyncEvent
    {
        TextEvent(TextId textSourceId) => TextSource = textSourceId;
        public TextEvent(TextId textSourceId, byte subId, TextLocation? location, CharacterId characterId)
        {
            TextSource = textSourceId;
            SubId = subId;
            Location = location;
            CharacterId = characterId;
        }

        public static TextEvent Serdes(TextEvent e, AssetMapping mapping, ISerializer s, TextId textSourceId)
        {
            e ??= new TextEvent(textSourceId);
            if (e.TextSource != textSourceId)
                throw new InvalidOperationException($"Called Serdes on a TextEvent with source id {e.TextSource} but passed in source id {textSourceId}");

            if (s == null) throw new ArgumentNullException(nameof(s));
            e.TextSource = textSourceId;
            e.Location = s.EnumU8(nameof(Location), e.Location ?? TextLocation.NoPortrait);
            int zeroed = s.UInt8(null, 0);
            zeroed += s.UInt8(null, 0);
            e.CharacterId = CharacterId.SerdesU8(nameof(CharacterId), e.CharacterId, e.CharacterType, mapping, s);
            e.SubId = s.UInt8(nameof(SubId), e.SubId);
            zeroed += s.UInt16(null, 0);
            zeroed += s.UInt16(null, 0);
            s.Assert(zeroed == 0, "TextEvent: Expected fields 2,3,6,8 to be 0");
            return e;
        }

        [EventPart("text")] public TextId TextSource { get; private set; }
        [EventPart("sub_id")] public byte SubId { get; private set; }
        [EventPart("location")] public TextLocation? Location { get; private set; }
        [EventPart("char")] public CharacterId CharacterId { get; private set; }

        AssetType CharacterType => Location switch
        {
            // TODO: Handle the other cases
            TextLocation.PortraitLeft => AssetType.PartyMember,
            _ => AssetType.Npc
        };

        public override MapEventType EventType => MapEventType.Text;
        public StringId ToId() => new(TextSource, SubId);
    }
}
