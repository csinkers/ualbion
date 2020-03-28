using System.Collections.Generic;
using System.Linq;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets
{
    public class EventChain
    {
        public EventChain(int id, TextSource textSource)
        {
            Id = id;
            TextSource = textSource;
            Enabled = true;
        }

        public int Id { get; }
        public TextSource TextSource { get; }
        public IList<IEventNode> Events { get; } = new List<IEventNode>();
        public IEventNode FirstEvent => Events[0];
        public bool Enabled { get; set; }

        public override string ToString() => $"Chain{Id} {(Enabled ? "" : "(Disabled)")}: {Events.Count} events starting at {Events.FirstOrDefault()?.Id}";
    }
}
