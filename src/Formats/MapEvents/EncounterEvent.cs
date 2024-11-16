using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("encounter")]
public class EncounterEvent : MapEvent
{
    EncounterEvent() { }
    public EncounterEvent(MonsterGroupId groupId, CombatBackgroundId backgroundId)
    {
        GroupId = groupId;
        BackgroundId = backgroundId;
    }

    public static EncounterEvent Serdes(EncounterEvent e, AssetMapping mapping, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new EncounterEvent();
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        e.GroupId = MonsterGroupId.SerdesU16(nameof(GroupId), e.GroupId, mapping, s);
        e.BackgroundId = CombatBackgroundId.SerdesU16(nameof(BackgroundId), e.BackgroundId, mapping, s);
        s.Assert(zeroes ==0, "EncounterEvent: Expected fields 1-5 to be 0");
        return e;
    }

    [EventPart("group")] public MonsterGroupId GroupId { get; private set; }
    [EventPart("background", true, "None")] public CombatBackgroundId BackgroundId { get; private set; }
    public override MapEventType EventType => MapEventType.Encounter;
}