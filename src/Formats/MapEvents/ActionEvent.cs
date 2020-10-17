using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.MapEvents
{
    public class ActionEvent : MapEvent
    {
        public static ActionEvent Serdes(ActionEvent e, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            var actionType = s.EnumU8(nameof(ActionType), e?.ActionType ?? 0);
            e ??= new ActionEvent();
            var assetType = actionType switch
            {
                var x when
                x == ActionType.AskAboutItem ||
                x == ActionType.UseItem ||
                x == ActionType.EquipItem ||
                x == ActionType.UnequipItem ||
                x == ActionType.PickupItem => AssetType.Item,
                ActionType.Word => AssetType.Word,
                ActionType.DialogueLine => AssetType.Unknown,
                _ => AssetType.Unknown
            };

            e.ActionType = actionType;
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.SmallArg = s.UInt8(nameof(SmallArg), e.SmallArg);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.Argument = AssetId.SerdesU16(nameof(Argument), e.Argument, assetType, mapping, s);
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
        public AssetId Argument { get; private set; }
        ushort Unk8 { get; set; }

        public override string ToString() => $"action {ActionType} Block:{SmallArg} From:{Argument} ({Unk2})"; // Unk2 is almost always 1
        public override MapEventType EventType => MapEventType.Action;
    }
}
