using SerdesNet;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets.Save
{
    public class VisitedEvent
    {
        public byte Unk0 { get; set; }
        public EventSetId EventSetId { get; set; }
        public ActionType Type { get; set; }
        public ushort Word { get; set; }

        public override string ToString() =>
            Type switch
            {
                ActionType.Word => $"{Unk0} {(int) EventSetId} {Type} {Word}={WordId}",
                ActionType.AskAboutItem => $"{Unk0} {(int) EventSetId} {Type} {ItemId}",
                ActionType.DialogueLine => $"{Unk0} {(int) EventSetId} {Type} Text:{Word / 256} Block:{Word % 256}",
                _ => $"{Unk0} {(int) EventSetId} {Type} {Word / 256} {Word % 256}"
            };
        public static VisitedEvent Serdes(int n, VisitedEvent u, ISerializer s)
        {
            if (s.Mode == SerializerMode.WritingAnnotated)
                s.Comment(u.ToString());

            u ??= new VisitedEvent();
            u.Unk0 = s.UInt8(nameof(Unk0), u.Unk0);
            u.EventSetId = s.EnumU16(nameof(EventSetId), u.EventSetId);
            u.Type = s.EnumU8(nameof(Type), u.Type);
            u.Word = s.UInt16(nameof(Word), u.Word);
            return u;
        }

        public WordId WordId => Word switch
        {
            { } x when x <= 193 => (WordId)(Word + 502),
            _ => (WordId) (Word + 503)
        };

        public ItemId ItemId => (ItemId)Word - 1;

        /*
         Logical to textual word id mapping clues:

            180 =>           (DDT)
            182 => 684 (502) (AI)
            183 => 685 (502) (Ned)
            189 => 691 (502) (over-c)
            190 => 692 (502) (complex)
            191 => 693 (502) (Snoopy)
            192 => 694 (502) (environmentalist)
            193 => 695 (502) (captain)
            194 => 697 (503) (Brandt)
            200 => 703 (503) (navigation officer)
            201 => 704 (503) (mathematician)
            202 => 705 (503) (flight)
         */
    }
}
