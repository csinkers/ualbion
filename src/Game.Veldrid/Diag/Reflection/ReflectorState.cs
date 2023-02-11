namespace UAlbion.Game.Veldrid.Diag.Reflection;

public readonly record struct ReflectorState(
    object Target,
    object Parent,
    int Index,
    ReflectorMetadata Meta);