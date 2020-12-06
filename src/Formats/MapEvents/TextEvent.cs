using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    public class TextEvent : MapEvent, ITextEvent, IAsyncEvent
    {
        public static TextEvent Serdes(TextEvent e, AssetMapping mapping, ISerializer s, TextId textSourceId)
        {
            e ??= new TextEvent(textSourceId);
            if (e.TextSourceId != textSourceId)
                throw new InvalidOperationException($"Called Serdes on a TextEvent with source id {e.TextSourceId} but passed in source id {textSourceId}");

            if (s == null) throw new ArgumentNullException(nameof(s));
            e.TextSourceId = textSourceId;
            e.Location = s.EnumU8(nameof(Location), e.Location ?? TextLocation.NoPortrait);
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.PortraitId = SpriteId.SerdesU8(nameof(PortraitId), e.PortraitId, AssetType.Portrait, mapping, s);
            e.TextId = s.UInt8(nameof(TextId), e.TextId);
            e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            return e;
        }

        public TextEvent(TextId textSourceId)
        {
            TextSourceId = textSourceId;
        }

        public TextEvent(TextId id, byte subId, TextLocation? location, SpriteId portrait)
        {
            TextSourceId = id;
            TextId = subId;
            Location = location;
            PortraitId = portrait;
        }

        [EventPart("text_id")] public byte TextId { get; private set; }
        [EventPart("location")] public TextLocation? Location { get; private set; }
        [EventPart("portrait")] public SpriteId PortraitId { get; private set; }

        public byte Unk2 { get; private set; }
        public byte Unk3 { get; private set; }
        public ushort Unk6 { get; private set; }
        public ushort Unk8 { get; private set; }

        public override string ToString() => $"text {TextSourceId}:{TextId} {Location} {PortraitId} ({Unk2} {Unk3} {Unk6} {Unk8})";
        public override MapEventType EventType => MapEventType.Text;
        public TextId TextSourceId { get; protected set; }
        public StringId ToId() => new StringId(TextSourceId, TextId);
    }
}
