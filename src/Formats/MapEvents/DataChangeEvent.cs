using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents;

[Event("change")]
public sealed class DataChangeEvent : MapEvent, IDataChangeEvent
{
    public override MapEventType EventType => MapEventType.DataChange;
    [EventPart("target")] public TargetId Target { get; private set; }
    [EventPart("prop")] public ChangeProperty ChangeProperty { get; private set; }
    [EventPart("op")] public NumericOperation Operation { get; private set; }
    [EventPart("amount", true, (ushort)0)] public ushort Amount { get; private set; }
    [EventPart("extra", true, (ushort)0)] public ushort Extra { get; private set; }
    [EventPart("random", true, false)] public bool IsRandom { get; private set; }

    DataChangeEvent() { }
    public DataChangeEvent(TargetId target, ChangeProperty property, NumericOperation op, ushort amount, ushort extra = 0, bool isRandom = false)
    {
        // change Target.PartyLeader Health AddPercentage 25
        // change Target.PartyLeader Health AddPercentage 25 Random
        Target = target;
        ChangeProperty = property;
        Operation = op;
        Amount = amount;
        Extra = extra;
        IsRandom = isRandom;
    }

    public static IDataChangeEvent Serdes(IDataChangeEvent existing, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        if (s.IsWriting() && existing == null) throw new ArgumentNullException(nameof(existing));

        var property = s.EnumU8(nameof(MapEvents.ChangeProperty), existing?.ChangeProperty ?? ChangeProperty.Unk0); // 1
        switch (property)
        {
            case ChangeProperty.Item:     return ChangeItemEvent.Serdes((ChangeItemEvent)existing, mapping, s);
            case ChangeProperty.Status:   return ChangeStatusEvent.Serdes((ChangeStatusEvent)existing, mapping, s);
            case ChangeProperty.Language: return ChangeLanguageEvent.Serdes((ChangeLanguageEvent)existing, mapping, s);
        }

        var e = (DataChangeEvent)(existing ?? new DataChangeEvent());
        var (targetType, targetId) = UnpackTargetId(e.Target);

        e.ChangeProperty = property;
        e.Operation = s.EnumU8(nameof(Operation), e.Operation); // 2
        targetType  = s.EnumU8(nameof(Target), targetType);     // 3
        e.IsRandom  = s.UInt8(nameof(IsRandom), (byte)(e.IsRandom ? 1 : 0)) != 0; // 4
        targetId    = s.UInt8("TargetId", targetId);    // 5
        e.Extra     = s.UInt16(nameof(Extra), e.Extra); // 6 - Item, Language, etc. Normally only handled in sub-classes.
        e.Amount = s.UInt16(nameof(Amount), e.Amount);  // 8
        e.Target = PackTargetId(targetType, targetId);

        return e;
    }

    public static (DataChangeTarget, byte) UnpackTargetId(TargetId target)
    {
        switch (target.Type)
        {
            case AssetType.Target:
                if (target == Base.Target.Leader) return (DataChangeTarget.PartyLeader, 0);
                if (target == Base.Target.Everyone) return (DataChangeTarget.AllMembers, 0);
                if (target == Base.Target.Inventory) return (DataChangeTarget.InventoryPic, 0);
                // if (target == Base.Target.CombatActor) return (DataChangeTarget.Unk3, 0);
                // if (target == Base.Target.CombatTarget) return (DataChangeTarget.Unk4, 0);
                // if (target == Base.Target.ConversationLeft) return (DataChangeTarget.Unk7, 0);
                // if (target == Base.Target.ConversationRight) return (DataChangeTarget.AllMembers, 0);
                return (DataChangeTarget.PartyLeader, 0);

            case AssetType.LocalNpc: return (DataChangeTarget.Npc, (byte)target.Id);
            case AssetType.Party: return (DataChangeTarget.SpecificMember, (byte)target.Id);
            default: return (DataChangeTarget.PartyLeader, 0);
        }
    }

    public static TargetId PackTargetId(DataChangeTarget targetType, byte targetId)
    {
        switch (targetType)
        {
            case DataChangeTarget.PartyLeader: return Base.Target.Leader;
            case DataChangeTarget.AllMembers: return Base.Target.Everyone;
            case DataChangeTarget.SpecificMember: return new PartyMemberId(AssetType.Party, targetId);
            case DataChangeTarget.Npc: return new TargetId(AssetType.LocalNpc, targetId);
            // case DataChangeTarget.Unk3: break;
            // case DataChangeTarget.Unk4: break;
            // case DataChangeTarget.InventoryPic: break;
            // case DataChangeTarget.Unk7: break;
            default: return TargetId.None;
        }
    }
}
