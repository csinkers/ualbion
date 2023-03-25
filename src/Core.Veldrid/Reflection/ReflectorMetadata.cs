using UAlbion.Api.Eventing;

namespace UAlbion.Core.Veldrid.Reflection;

public delegate object ReflectorGetter(in ReflectorState state);
public delegate void ReflectorSetter(in ReflectorState state, object value);
public record ReflectorMetadata(
    string Name,
    ReflectorGetter Getter,
    ReflectorSetter Setter,
    DiagEditAttribute Options);