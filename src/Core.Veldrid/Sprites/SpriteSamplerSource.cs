using System;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Sprites
{
    public sealed class SpriteSamplerSource : ServiceComponent<ISpriteSamplerSource>, ISpriteSamplerSource, IDisposable
    {
        readonly SamplerHolder LinearSampler;
        readonly SamplerHolder PointSampler;
        public SpriteSamplerSource()
        {
            LinearSampler = new SamplerHolder
            {
                AddressModeU = SamplerAddressMode.Clamp,
                AddressModeV = SamplerAddressMode.Clamp,
                AddressModeW = SamplerAddressMode.Clamp,
                BorderColor = SamplerBorderColor.TransparentBlack,
                Filter = SamplerFilter.MinLinear_MagLinear_MipLinear,
            };

            PointSampler = new SamplerHolder
            {
                AddressModeU = SamplerAddressMode.Clamp,
                AddressModeV = SamplerAddressMode.Clamp,
                AddressModeW = SamplerAddressMode.Clamp,
                BorderColor = SamplerBorderColor.TransparentBlack,
                Filter = SamplerFilter.MinPoint_MagPoint_MipPoint,
            };
            AttachChild(LinearSampler);
            AttachChild(PointSampler);
        }

        public ISamplerHolder GetSampler(SpriteSampler sampler) =>
            sampler switch
            {
                SpriteSampler.TriLinear => LinearSampler,
                SpriteSampler.Point => PointSampler,
                _ => throw new ArgumentOutOfRangeException(nameof(sampler), "Unexpected sprite sampler \"{sampler}\"")
            };

        public void Dispose()
        {
            LinearSampler?.Dispose();
            PointSampler?.Dispose();
        }
    }
}