using System;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Game.Tests;

class MockCollisionManager : ICollisionManager
{
    readonly Func<int, int, Passability> _func;

    public MockCollisionManager(Func<int, int, Passability> func) => _func = func ?? throw new ArgumentNullException(nameof(func));
    public bool IsOccupied(int tx, int ty) => GetPassability(tx, ty) != Passability.Passable;
    public Passability GetPassability(int tx, int ty) => _func(tx, ty);
    public void Register(IMovementCollider collider) { }
    public void Unregister(IMovementCollider collider) { }
}