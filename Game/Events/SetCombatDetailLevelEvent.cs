using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("set_combat_detail")]
    public class SetCombatDetailLevelEvent : GameEvent
    {
        public SetCombatDetailLevelEvent(int value)
        {
            Value = value;
        }

        [EventPart("value")]
        public int Value { get; }
    }
}