using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public class ActionEvent : MapEvent
    {
        public static ActionEvent Serdes(ActionEvent e, ISerializer s)
        {
            var actionType = s.EnumU8(nameof(ActionType), e?.ActionType ?? 0);
            e ??= 
                actionType switch
                {
                    var x when
                        x == ActionType.UseItem ||
                        x == ActionType.EquipItem ||
                        x == ActionType.UnequipItem ||
                        x == ActionType.PickupItem => new ItemActionEvent(),
                    ActionType.Word => new WordActionEvent(),
                    _ => new ActionEvent()
                };

            e.ActionType = actionType;
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);

            ApiUtil.Assert(e.Unk2 == 1 || ((int)e.ActionType == 14 && e.Unk2 == 2));
            ApiUtil.Assert(e.Unk4 == 0);
            ApiUtil.Assert(e.Unk5 == 0);
            ApiUtil.Assert(e.Unk8 == 0);
            return e;
        }

        public ActionType ActionType { get; private set; }
        public byte Unk2 { get; private set; } // Always 1, unless ActionType == 14 (in which cas it is 2)
        public byte Unk3 { get; private set; } // BLOK?
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }
        protected ushort Unk6 { get; private set; }
        ushort Unk8 { get; set; }

        public override string ToString() => $"action {ActionType} {Unk3}: {Unk6} ({Unk2})";
        public override MapEventType EventType => MapEventType.Action;
    }

    public class ItemActionEvent : ActionEvent
    {
        public ItemId ItemId => (ItemId)(Unk6 + 1);
        public override string ToString() => $"action {ActionType} {Unk3}: {ItemId} ({Unk2}";
    }

    public class WordActionEvent : ActionEvent
    {
        public WordId WordId => (WordId)(Unk6 + 1);
        public override string ToString() => $"action {ActionType} {Unk3}: {WordId} ({Unk2}";
    }
}
