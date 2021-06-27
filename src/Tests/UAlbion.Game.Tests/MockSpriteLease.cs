using System;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Tests
{
    public class MockSpriteLease : ISpriteLease
    {
        public MockSpriteLease(SpriteKey key, int length)
        {
            Key = key;
            Length = length;
        }

        public SpriteKey Key { get; }
        public int Length { get; }
        public void Update(int index, Vector3 position, Vector2 size, Region region, SpriteFlags flags) { }
        public void Update(int index, Vector3 position, Vector2 size, int regionIndex, SpriteFlags flags) { }
        public void UpdateFlags(int index, SpriteFlags flags, SpriteFlags? mask = null) { }
        public void OffsetAll(Vector3 offset) { }
        public IWeakSpriteReference MakeWeakReference(int index)
        {
            throw new NotImplementedException();
        }

        public void Access<T>(ISpriteLease.LeaseAccessDelegate<T> mutatorFunc, T context)
        {
            throw new NotImplementedException();
        }

        public Span<SpriteInstanceData> Lock(ref bool lockWasTaken)
        {
            throw new NotImplementedException();
        }

        public void Unlock(bool lockWasTaken)
        {
            throw new NotImplementedException();
        }

        public void Dispose() { }
    }
}