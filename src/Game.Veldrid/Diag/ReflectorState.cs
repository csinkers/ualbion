namespace UAlbion.Game.Veldrid.Diag;

public readonly record struct ReflectorState(
    object Target,
    object Parent,
    int Index,
    ReflectorMetadata Meta);