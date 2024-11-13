using System;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Veldrid.Reflection;

public delegate object ReflectorGetter(in ReflectorState state);
public delegate void ReflectorSetter(in ReflectorState state, object value);

/// <summary>
/// Represents metadata for inspecting the state of a particular property on a particular type.
/// </summary>
/// <param name="Name">The name of the property</param>
/// <param name="ParentType">The type that the property belongs to</param>
/// <param name="ValueType">The type of the property itself</param>
/// <param name="Getter">Method to retrieve the current value of the property, given an instance of the parent type</param>
/// <param name="Setter">Method to update the value of the property, given an instance of the parent type</param>
/// <param name="Options">Details of any overridden inspector behaviours for this property. Null if defaults are to be used.</param>
public record ReflectorMetadata(
    string Name,
    Type ParentType,
    Type ValueType,
    ReflectorGetter Getter,
    ReflectorSetter Setter,
    DiagEditAttribute Options)
{
    public DiagEditAttribute Options { get; set; } = Options;
}