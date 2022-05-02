using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;

namespace UAlbion.Formats.MapEvents;

[Event("action")]
public class ActionEvent : MapEvent
{
    ActionEvent() { }
    [EventPart("type")] public ActionType ActionType { get; private set; }
    [EventPart("block", true, (byte)0)] public byte Block { get; private set; } // Item Class, 255 for 'any'
    [EventPart("arg", true, "None")] public AssetId Argument { get; private set; }
    [EventPart("unk2", true, (byte)1)] public byte Unk2 { get; private set; } // Always 1, unless ActionType == 14 (in which case it is 2)

    public ActionEvent(ActionType actionType, byte block, AssetId arg, byte unk2)
    {
        ActionType = actionType;
        Unk2 = unk2;
        Block = block;
        Argument = arg;
    }

    public static ActionEvent Serdes(ActionEvent e, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        var actionType = s.EnumU8(nameof(ActionType), e?.ActionType ?? 0);
        e ??= new ActionEvent();
        var assetType = actionType switch
        {
            ActionType.AskAboutItem 
                or ActionType.UseItem 
                or ActionType.EquipItem 
                or ActionType.UnequipItem 
                or ActionType.PickupItem => AssetType.Item,
            ActionType.Word => AssetType.Word,
            ActionType.DialogueLine => AssetType.Unknown,
            _ => AssetType.Unknown
        };

        e.ActionType = actionType;
        e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
        e.Block = s.UInt8(nameof(Block), e.Block);
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

    byte Unk4 { get; set; }
    byte Unk5 { get; set; }
    ushort Unk8 { get; set; }

    public override MapEventType EventType => MapEventType.Action;
}