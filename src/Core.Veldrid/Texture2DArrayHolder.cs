using System;
using UAlbion.Api.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid
{
    public sealed class Texture2DArrayHolder : TextureHolder, ITextureArrayHolder
    {
        public Texture2DArrayHolder(ITexture texture) : base(texture) {}
        protected override Texture Create(GraphicsDevice device) =>
            Texture switch
            { // Note: No automatic mip-mapping for 8-bit, blending/interpolation in palette-based images typically results in nonsense.
                // TODO: Custom mip-mapping using nearest matches in the palette
                IReadOnlyTexture<byte> eightBitArray => VeldridTexture.CreateArrayTexture(device, TextureUsage.Sampled, eightBitArray),
                IReadOnlyTexture<uint> trueColorArray => VeldridTexture.CreateArrayTexture(device, TextureUsage.Sampled | TextureUsage.GenerateMipmaps, trueColorArray),
                _ => throw new NotSupportedException($"Image format {Texture.GetType().GetGenericArguments()[0].Name} not currently supported")
            };
    }
}
