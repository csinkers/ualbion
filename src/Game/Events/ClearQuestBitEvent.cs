using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events
{
    [Event("clear_quest_bit")] // USED IN SCRIPT
    public class ClearQuestBitEvent : GameEvent
    {
        public ClearQuestBitEvent(SwitchId questId) { QuestId = questId; }
        [EventPart("switch")] public SwitchId QuestId { get; }
    }
}
