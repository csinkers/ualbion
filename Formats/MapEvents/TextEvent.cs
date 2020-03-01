using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    [Event("text")]
    public class TextEvent : AsyncEvent, IMapEvent
    {
        public static TextEvent Serdes(TextEvent e, ISerializer s)
        {
            e ??= new TextEvent();
            e.Location = s.EnumU8(nameof(Location), e.Location);
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.PortraitId = (SmallPortraitId)StoreIncremented.Serdes(nameof(PortraitId), (byte)e.PortraitId, s.UInt8);
            e.TextId = s.UInt8(nameof(TextId), e.TextId);
            e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            return e;
        }

        public enum TextLocation : byte
        {
            TextInWindow = 0,
            TextInWindowWithPortrait = 2,
            TextInWindowWithPortrait2 = 10, // Not sure how this one differs
            // TextInWindowWithNpcPortrait,
            // AddDialogOption,
            QuickInfo = 6,
            // DialogQuestion,
            // DialogResponse,
            // AddDefaultDialogOption,
            // ListDefaultDialogOptions
        }

        TextEvent() { }

        public TextEvent(byte textId, TextLocation location, SmallPortraitId portrait)
        {
            TextId = textId;
            Location = location;
            PortraitId = portrait;
        }

        protected override AsyncEvent Clone() => new TextEvent(TextId, Location, PortraitId);

        [EventPart("text_id")] public byte TextId { get; private set; }
        [EventPart("location")] public TextLocation Location { get; private set; }
        [EventPart("portrait")] public SmallPortraitId PortraitId { get; private set; }
        public byte Unk2 { get; private set; }
        public byte Unk3 { get; private set; }
        public ushort Unk6 { get; private set; }
        public ushort Unk8 { get; private set; }
        public override string ToString() => $"text {TextId} {Location} {PortraitId} ({Unk2} {Unk3} {Unk6} {Unk8})";
        public MapEventType EventType => MapEventType.Text;
    }
}
