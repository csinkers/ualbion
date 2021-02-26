using System;
using System.Numerics;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Game.Tests
{
    class MockCollisionManager : ICollisionManager
    {
        readonly Func<int, int, Passability> _func;

        public MockCollisionManager(Func<int, int, Passability> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public bool IsOccupied(Vector2 tilePosition) => GetPassability(tilePosition) != Passability.Passable;
        public Passability GetPassability(Vector2 tilePosition) => _func((int)tilePosition.X, (int)tilePosition.Y);
        public void Register(IMovementCollider collider) { }
        public void Unregister(IMovementCollider collider) { }
    }
}