namespace UAlbion.Game.Diag;

public readonly record struct ReflectorState(string Name, object Target, TypeReflector Reflector, object Parent, int CollectionIndex);