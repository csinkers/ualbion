using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("update", "Run the game clock for the specified number of slow-clock cycles")] // USED IN SCRIPT
    public class UpdateEvent : GameEvent, IAsyncEvent
    {
        public UpdateEvent(int cycles) => Cycles = cycles;

        [EventPart("cycles", "The number of slow-clock cycles to update for")]
        public int Cycles { get; }
    }
}