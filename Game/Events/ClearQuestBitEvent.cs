using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("clear_quest_bit")]
    public class ClearQuestBitEvent : GameEvent
    {
        public ClearQuestBitEvent(int questId) { QuestId = questId; }
        [EventPart("questId")] public int QuestId { get; }
    }
}
