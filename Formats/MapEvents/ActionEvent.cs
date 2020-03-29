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
                    x == ActionType.AskAboutItem ||
                    x == ActionType.UseItem ||
                    x == ActionType.EquipItem ||
                    x == ActionType.UnequipItem ||
                    x == ActionType.PickupItem => new ItemActionEvent(),
                    ActionType.Word => new WordActionEvent(),
                    ActionType.DialogueLine => new DialogueLineActionEvent(),
                    _ => new ActionEvent()
                };

            e.ActionType = actionType;
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.SmallArg = s.UInt8(nameof(SmallArg), e.SmallArg);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.LargeArg = s.UInt16(nameof(LargeArg), e.LargeArg);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);

            ApiUtil.Assert(e.Unk2 == 1 || ((int)e.ActionType == 14 && e.Unk2 == 2));
            ApiUtil.Assert(e.Unk4 == 0);
            ApiUtil.Assert(e.Unk5 == 0);
            ApiUtil.Assert(e.Unk8 == 0);
            return e;
        }

        public ActionType ActionType { get; private set; }
        public byte Unk2 { get; private set; } // Always 1, unless ActionType == 14 (in which cas it is 2)
        public byte SmallArg { get; private set; } // Item Class, 255 for 'any'
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }
        protected ushort LargeArg { get; private set; }
        ushort Unk8 { get; set; }

        public override string ToString() => $"action {ActionType} {SmallArg}: {LargeArg} ({Unk2})";
        public override MapEventType EventType => MapEventType.Action;
    }

    public class DialogueLineActionEvent : ActionEvent
    {
        public int BlockId => SmallArg;
        public int TextId => LargeArg;
    }

    public class ItemActionEvent : ActionEvent
    {
        public ItemId ItemId => (ItemId)(LargeArg - 1);
        public override string ToString() => $"action {ActionType} {SmallArg}: {ItemId} ({Unk2})";
    }

    public class WordActionEvent : ActionEvent
    {
        public WordId WordId => (WordId)(LargeArg + 1);
        public override string ToString() => $"action {ActionType} {SmallArg}: {WordId} ({Unk2})";
    }
}
