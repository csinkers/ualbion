using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("update", "Run the game clock for the specified number of slow-clock cycles")]
    public class UpdateEvent : AsyncEvent
    {
        public UpdateEvent(int cycles) => Cycles = cycles;

        [EventPart("cycles", "The number of slow-clock cycles to update for")]
        public int Cycles { get; }
    }
}