using System;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Tests;

public class MockSpriteBatch<TInstance> : RenderableBatch<SpriteKey, TInstance>
    where TInstance : unmanaged
{
    TInstance[] _instances = new TInstance[MinSize];
    public MockSpriteBatch(SpriteKey key, Action<Span<TInstance>> disableInstancesFunc) : base(key, disableInstancesFunc) { }
    protected override ReadOnlySpan<TInstance> ReadOnlyInstances => _instances;
    protected override Span<TInstance> MutableInstances => _instances;
    protected override void Resize(int instanceCount)
    {
        if (instanceCount == _instances.Length) return;
        var old = _instances;
        _instances = new TInstance[instanceCount];
        Array.Copy(old, _instances, Math.Min(old.Length, _instances.Length));
    }
}
