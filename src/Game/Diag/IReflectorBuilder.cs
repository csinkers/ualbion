using System;

namespace UAlbion.Game.Diag;

public interface IReflectorBuilder
{
    Reflector Build(ReflectorManager manager, string name, Type type);
}