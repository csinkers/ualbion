using System.Collections.Generic;
using System.Linq;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets
{
    public class EventChain
    {
        public EventChain(int id)
        {
            Id = id;
            Enabled = true;
        }

        public int Id { get; }
        public IList<IEventNode> Events { get; } = new List<IEventNode>();
        public IMapEvent FirstEvent => Events[0].Event;
        public bool Enabled { get; set; }

        public override string ToString() => $"Chain{Id} {(Enabled ? "" : "(Disabled)")}: {Events.Count} events starting at {Events.FirstOrDefault()?.Id}";
    }
}