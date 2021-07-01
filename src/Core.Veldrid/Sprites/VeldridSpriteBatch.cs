﻿using System;
using System.Runtime.InteropServices;
using UAlbion.Core.Visual;
using Veldrid;

namespace UAlbion.Core.Veldrid.Sprites
{
    public class VeldridSpriteBatch : SpriteBatch
    {
        public VeldridSpriteBatch(SpriteKey key) : base(key)
        {
            Instances = new MultiBuffer<GpuSpriteInstanceData>(MinSize, BufferUsage.VertexBuffer, $"B_Inst:{Name}");
            Uniform = new SingleBuffer<SpriteUniform>(new SpriteUniform
            {
                Flags = Key.Flags,
                TextureWidth = key.Texture.Width,
                TextureHeight = key.Texture.Height
            }, BufferUsage.UniformBuffer, $"B_SpriteUniform:{Name}");
            AttachChild(Instances);
            AttachChild(Uniform);
        }

        internal MultiBuffer<GpuSpriteInstanceData> Instances { get; }
        internal SingleBuffer<SpriteUniform> Uniform { get; }
        internal SpriteArraySet SpriteResources { get; private set; }
        protected override void Subscribed()
        {
            var samplerSource = Resolve<ISpriteSamplerSource>();
            SpriteResources = new SpriteArraySet
            {
                Name = $"RS_Sprite:{Key.Texture.Name}",
                Texture = Resolve<ITextureSource>().GetArrayTexture(Key.Texture),
                Sampler = samplerSource.GetSampler(Key.Sampler),
                Uniform = Uniform
            };
            AttachChild(SpriteResources);
        }

        protected override void Unsubscribed()
        {
            CleanupSet();
        }

        protected override ReadOnlySpan<SpriteInstanceData> ReadOnlySprites =>
            MemoryMarshal.Cast<GpuSpriteInstanceData, SpriteInstanceData>(Instances.Data);

        protected override Span<SpriteInstanceData> MutableSprites 
            => MemoryMarshal.Cast<GpuSpriteInstanceData, SpriteInstanceData>(Instances.Borrow());

        protected override void Resize(int instanceCount) 
            => Instances.Resize(instanceCount);

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
}

