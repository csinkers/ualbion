using System;
using System.Numerics;
using System.Runtime.InteropServices;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Visual;
using Veldrid;

namespace UAlbion.Core.Veldrid.Sprites;

public class VeldridSpriteBatch<TInstance, TGpuInstance> : RenderableBatch<SpriteKey, TInstance>
    where TGpuInstance : unmanaged 
    where TInstance : unmanaged
{
    public VeldridSpriteBatch(SpriteKey key) : base(key)
    {
        unsafe
        {
            if (sizeof(TInstance) != sizeof(TGpuInstance))
            {
                throw new InvalidOperationException(
                    $"Sprite instance type {typeof(TInstance).Name} has size {sizeof(TInstance)}, " +
                    $"which does not match the GPU instance type {typeof(TGpuInstance).Name} of size {sizeof(TGpuInstance)}");
            }
        }

        Instances = new MultiBuffer<TGpuInstance>(MinSize, BufferUsage.VertexBuffer, $"B_Inst:{Name}");
        Uniform = new SingleBuffer<SpriteUniform>(new SpriteUniform
        {
            Flags = Key.Flags,
            TextureSize = new Vector2(key.Texture.Width, key.Texture.Height)
        }, BufferUsage.UniformBuffer, $"B_SpriteUniform:{Name}");
        AttachChild(Instances);
        AttachChild(Uniform);
    }

    internal MultiBuffer<TGpuInstance> Instances { get; }
    internal SingleBuffer<SpriteUniform> Uniform { get; }
    internal SpriteSet SpriteResources { get; private set; }
    protected override void Subscribed()
    {
        var samplerSource = Resolve<ISpriteSamplerSource>();
        bool isArray = (Key.Flags & SpriteKeyFlags.UseArrayTexture) != 0;
        var source = Resolve<ITextureSource>();

        SpriteResources = new SpriteSet
        {
            Name = $"RS_Sprite:{Key.Texture.Name}",
            Texture = isArray ? source.GetDummySimpleTexture() : source.GetSimpleTexture(Key.Texture),
            TextureArray = isArray ? source.GetArrayTexture(Key.Texture) : source.GetDummyArrayTexture(),
            Sampler = samplerSource.GetSampler(Key.Sampler),
            Uniform = Uniform
        };
        AttachChild(SpriteResources);
    }

    protected override void Unsubscribed() => CleanupSet();
    protected override ReadOnlySpan<TInstance> ReadOnlyInstances => MemoryMarshal.Cast<TGpuInstance, TInstance>(Instances.Data);
    protected override Span<TInstance> MutableInstances => MemoryMarshal.Cast<TGpuInstance, TInstance>(Instances.Borrow());
    protected override void Resize(int instanceCount) => Instances.Resize(instanceCount);

    void CleanupSet()
    {
        SpriteResources.Dispose();
        RemoveChild(SpriteResources);
        SpriteResources = null;
    }

    protected override void Dispose(bool disposing)
    {
        CleanupSet();
        Instances.Dispose();
        Uniform.Dispose();
        base.Dispose(disposing);
    }
}