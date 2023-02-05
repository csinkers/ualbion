namespace UAlbion.Game.Diag;

public readonly record struct ReflectorState(
    string Name,
    object Target,
    Reflector Reflector,
    object Parent,
    int CollectionIndex);