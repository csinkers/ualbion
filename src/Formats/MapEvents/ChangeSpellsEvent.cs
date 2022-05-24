using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("change_spells")]
public class ChangeSpellsEvent : MapEvent, IDataChangeEvent
{
    public override MapEventType EventType => MapEventType.DataChange;
    public ChangeProperty ChangeProperty => ChangeProperty.KnownSpells;
    [EventPart("target")] public TargetId Target { get; private set; }
    [EventPart("school")] public SpellClass School { get; private set; }
    [EventPart("num")] public ushort SpellNumber { get; private set; }
    [EventPart("op")] public NumericOperation Operation { get; private set; }

    ChangeSpellsEvent() { }
    public ChangeSpellsEvent(TargetId target, SpellClass school, ushort num, NumericOperation operation)
    {
        School = school;
        SpellNumber = num;
        Target = target;
        Operation = operation;
    }

    public static ChangeSpellsEvent Serdes(ChangeSpellsEvent e, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new ChangeSpellsEvent();
        var (targetType, targetId) = DataChangeEvent.UnpackTargetId(e.Target);

        e.Operation   = s.EnumU8(nameof(Operation), e.Operation); // 2
        targetType    = s.EnumU8(nameof(Target), targetType);     // 3
        s.UInt8("Unused", 0);                                     // 4
        targetId      = s.UInt8("TargetId", targetId);            // 5
        e.School      = s.EnumU16("SpellSchool", e.School);       // 6
        e.SpellNumber = s.UInt16("SpellNumber", e.SpellNumber);   // 8
        e.Target      = DataChangeEvent.PackTargetId(targetType, targetId);
        return e;
    }
}