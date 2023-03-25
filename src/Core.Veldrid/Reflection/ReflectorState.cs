namespace UAlbion.Core.Veldrid.Reflection;

public readonly record struct ReflectorState(
    object Target,
    object Parent,
    int Index,
    ReflectorMetadata Meta);