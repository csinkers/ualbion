using UAlbion.Api.Eventing;

namespace UAlbion.Scripting;

public interface IEventFormatter
{
    string Format(IEvent e, bool useNumeric);
}