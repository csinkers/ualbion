using System;
using System.Runtime.InteropServices;
using UAlbion.Core.Visual;
using Veldrid;

namespace UAlbion.Core.Veldrid.Sprites
{
    public class VeldridSpriteBatch : SpriteBatch
    {
        public VeldridSpriteBatch(SpriteKey key) : base(key)
        {
            Instances = AttachChild(new MultiBuffer<GpuSpriteInstanceData>(MinSize, BufferUsage.VertexBuffer, $"B_Inst:{Name}"));
            Uniform = AttachChild(new SingleBuffer<SpriteUniform>(new SpriteUniform
            {
                Flags = Key.Flags,
                TextureWidth = key.Texture.Width,
                TextureHeight = key.Texture.Height
            }, BufferUsage.UniformBuffer, $"B_SpriteUniform:{Name}"));
        }

        internal MultiBuffer<GpuSpriteInstanceData> Instances { get; }
        internal SingleBuffer<SpriteUniform> Uniform { get; }
        internal SpriteArraySet SpriteResources { get; private set; }
        protected override void Subscribed()
        {
            var samplerSource = Resolve<ISpriteSamplerSource>();
            SpriteResources = AttachChild(new SpriteArraySet
            {
                Name = $"RS_Sprite:{Key.Texture.Name}",
                Texture = Resolve<ITextureSource>().GetArrayTexture(Key.Texture),
                Sampler = samplerSource.Get(Key.Sampler),
                Uniform = Uniform
            });
        }

        protected override void Unsubscribed() => RemoveChild(SpriteResources);

        protected override ReadOnlySpan<SpriteInstanceData> ReadOnlySprites =>
            MemoryMarshal.Cast<GpuSpriteInstanceData, SpriteInstanceData>(Instances.Data);

        protected override Span<SpriteInstanceData> MutableSprites 
            => MemoryMarshal.Cast<GpuSpriteInstanceData, SpriteInstanceData>(Instances.Borrow());

        protected override void Resize(int instanceCount) 
            => Instances.Resize(instanceCount);
    }
}

