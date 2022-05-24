using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.ScriptEvents;

[Event("clear_quest_bit")] // USED IN SCRIPT
public class ClearQuestBitEvent : Event
{
    public ClearQuestBitEvent(SwitchId questId) { QuestId = questId; }
    [EventPart("switch")] public SwitchId QuestId { get; }
}