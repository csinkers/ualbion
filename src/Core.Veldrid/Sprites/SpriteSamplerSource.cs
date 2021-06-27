using System;
using UAlbion.Core.Visual;
using Veldrid;

namespace UAlbion.Core.Veldrid.Sprites
{
    public class SpriteSamplerSource : Component, ISpriteSamplerSource
    {
        readonly SamplerHolder LinearSampler;
        readonly SamplerHolder PointSampler;
        public SpriteSamplerSource()
        {
            LinearSampler = AttachChild(new SamplerHolder
            {
                AddressModeU = SamplerAddressMode.Clamp,
                AddressModeV = SamplerAddressMode.Clamp,
                AddressModeW = SamplerAddressMode.Clamp,
                BorderColor = SamplerBorderColor.TransparentBlack,
                Filter = SamplerFilter.MinLinear_MagLinear_MipLinear,
            });

            PointSampler = AttachChild(new SamplerHolder
            {
                AddressModeU = SamplerAddressMode.Clamp,
                AddressModeV = SamplerAddressMode.Clamp,
                AddressModeW = SamplerAddressMode.Clamp,
                BorderColor = SamplerBorderColor.TransparentBlack,
                Filter = SamplerFilter.MinPoint_MagPoint_MipPoint,
            });
        }

        public SamplerHolder Get(SpriteSampler sampler) =>
            sampler switch
            {
                SpriteSampler.Linear => LinearSampler,
                SpriteSampler.Point => PointSampler,
                _ => throw new ArgumentOutOfRangeException(nameof(sampler), "Unexpected sprite sampler \"{sampler}\"")
            };
    }
}