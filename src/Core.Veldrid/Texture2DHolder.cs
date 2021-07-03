using System;
using UAlbion.Api.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid
{
    public sealed class Texture2DHolder : TextureHolder, ITextureHolder
    {
        public Texture2DHolder(ITexture texture) : base(texture) { }
        protected override Texture Create(GraphicsDevice device)
        {
            var deviceTexture = Texture switch
                { // Note: No automatic mip-mapping for 8-bit, blending/interpolation in palette-based images typically results in nonsense.
                  // TODO: Custom mip-mapping using nearest matches in the palette
                    IReadOnlyTexture<byte> eightBit => VeldridTexture.CreateSimpleTexture(device, TextureUsage.Sampled, eightBit),
                    IReadOnlyTexture<uint> trueColor => VeldridTexture.CreateSimpleTexture(device, TextureUsage.Sampled | TextureUsage.GenerateMipmaps, trueColor),
                    _ => throw new NotSupportedException($"Image format {Texture.GetType().GetGenericArguments()[0].Name} not currently supported")
                };
            return deviceTexture;
        }
    }
}
