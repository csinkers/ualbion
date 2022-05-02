using UAlbion.Api.Eventing;
using UAlbion.Game.Text;

namespace UAlbion.Game.Events;

public class DescriptionTextEvent : GameEvent, IVerboseEvent
{
    public DescriptionTextEvent(IText source) { Source = source; }
    public IText Source { get; }
}