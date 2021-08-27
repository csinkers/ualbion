using System;
using System.Globalization;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets.Save
{
    public class VisitedEvent
    {
        public const int SizeOnDisk = 6;
        public byte Unk0 { get; set; }
        public EventSetId EventSetId { get; set; }
        public ActionType Type { get; set; }

        public static VisitedEvent Serdes(int n, VisitedEvent u, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            u ??= new VisitedEvent();
            s.Begin();
            u.Unk0 = s.UInt8(nameof(Unk0), u.Unk0);
            u.EventSetId = EventSetId.SerdesU16(nameof(EventSetId), u.EventSetId, mapping, s);
            u.Type = s.EnumU8(nameof(Type), u.Type);

            switch (u.Type)
            {
                case ActionType.AskAboutItem:
                case ActionType.UseItem:
                case ActionType.EquipItem:
                case ActionType.UnequipItem:
                case ActionType.PickupItem:
                    u._value = ItemId.SerdesU16("Value", ItemId.FromUInt32(u._value), AssetType.Item, mapping, s).ToUInt32();
                    break;
                case ActionType.Word:
                    u._value = WordId.SerdesU16("Value", WordId.FromUInt32(u._value), mapping, s).ToUInt32();
                    break;
                default:
                    u._value = s.UInt16("Value", (ushort)u._value);
                    break;
            }

            if (s.IsCommenting())
                s.Comment(u.ToString());
            s.End();
            return u;
        }

        uint _value;

        [JsonInclude] public uint Value { get => _value; private set => _value = value; }

        [JsonIgnore]
        public WordId WordId =>
            Type == ActionType.Word
                ? WordId.FromUInt32(_value)
                : throw new InvalidOperationException("Tried to retrieve WordId of a non-word VisitedEvent");
        /*
        public WordId WordId => Word switch
        {
            // TODO: Ugh... finish reversing this and then fix up via asset mappings?
            { } x when x <= 193 => (WordId)(Word + 502), 
            _ => (WordId) (Word + 503)
        }; */

        [JsonIgnore]
        public ItemId ItemId => 
            Type is ActionType.AskAboutItem 
                or ActionType.UseItem 
                or ActionType.EquipItem 
                or ActionType.UnequipItem 
                or ActionType.PickupItem
                ? ItemId.FromUInt32(_value)
                : throw new InvalidOperationException("Tried to retrieve ItemId of a non-item VisitedEvent");

        string ItemString =>
            Type switch
            {
                ActionType.AskAboutItem => ItemId.ToString(),
                ActionType.UseItem => ItemId.ToString(),
                ActionType.EquipItem => ItemId.ToString(),
                ActionType.UnequipItem => ItemId.ToString(),
                ActionType.PickupItem => ItemId.ToString(),
                ActionType.Word => WordId.ToString(),
                ActionType.DialogueLine => $"Text:{_value >> 8} Block:{_value & 0xff}",
                _ => _value.ToString(CultureInfo.InvariantCulture)
            };

        public override string ToString() => $"{Unk0} {EventSetId} {Type} {ItemString}";

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
