using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
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
        [JsonIgnore] public IEventNode FirstEvent => Events.Count == 0 ? null : Events[0];
        [JsonIgnore] public bool Enabled { get; set; }

        public override string ToString() => $"Chain{Id} {(Enabled ? "" : "(Disabled)")}: {Events.Count} events @ {Events.FirstOrDefault()?.Id}: {FirstEvent}";
    }
}
