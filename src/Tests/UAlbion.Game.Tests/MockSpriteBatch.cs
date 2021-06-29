using System;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Tests
{
    public class MockSpriteBatch : SpriteBatch
    {
        SpriteInstanceData[] _instances = new SpriteInstanceData[MinSize];
        public MockSpriteBatch(SpriteKey key) : base(key) { } 
        protected override ReadOnlySpan<SpriteInstanceData> ReadOnlySprites => _instances;
        protected override Span<SpriteInstanceData> MutableSprites => _instances;
        protected override void Resize(int instanceCount)
        {
            if (instanceCount == _instances.Length) return;
            var old = _instances;
            _instances = new SpriteInstanceData[instanceCount];
            Array.Copy(old, _instances, Math.Min(old.Length, _instances.Length));
        }
    }
}