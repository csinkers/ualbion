using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace UAlbion.Api.Eventing;

[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Don't care about VB")]
public abstract class Event : IEvent // Contains no fields, only helper methods for reflection-based parsing and serialization.
{
    public static IEvent Parse(string s, out string error) => EventSerializer.Instance.Parse(s, out error);
    public static void AddEventsFromAssembly(Assembly assembly) => EventSerializer.Instance.AddEventsFromAssembly(assembly);

    public override string ToString() => EventSerializer.Instance.ToString(this);
    public void Format(IScriptBuilder builder) => EventSerializer.Instance.Format(builder, this);
}