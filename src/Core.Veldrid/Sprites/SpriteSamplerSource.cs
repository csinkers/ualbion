using System;
using UAlbion.Api.Eventing;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Sprites;

public sealed class SpriteSamplerSource : ServiceComponent<ISpriteSamplerSource>, ISpriteSamplerSource, IDisposable
{
    readonly SamplerHolder _linearSampler;
    readonly SamplerHolder _pointSampler;
    public SpriteSamplerSource()
    {
        _linearSampler = new SamplerHolder
        {
            AddressModeU = SamplerAddressMode.Clamp,
            AddressModeV = SamplerAddressMode.Clamp,
            AddressModeW = SamplerAddressMode.Clamp,
            BorderColor = SamplerBorderColor.TransparentBlack,
            Filter = SamplerFilter.MinLinear_MagLinear_MipLinear,
        };

        _pointSampler = new SamplerHolder
        {
            AddressModeU = SamplerAddressMode.Clamp,
            AddressModeV = SamplerAddressMode.Clamp,
            AddressModeW = SamplerAddressMode.Clamp,
            BorderColor = SamplerBorderColor.TransparentBlack,
            Filter = SamplerFilter.MinPoint_MagPoint_MipPoint,
        };
        AttachChild(_linearSampler);
        AttachChild(_pointSampler);
    }

    public ISamplerHolder GetSampler(SpriteSampler sampler) =>
        sampler switch
        {
            SpriteSampler.TriLinear => _linearSampler,
            SpriteSampler.Point => _pointSampler,
            _ => throw new ArgumentOutOfRangeException(nameof(sampler), "Unexpected sprite sampler \"{sampler}\"")
        };

    public void Dispose()
    {
        _linearSampler?.Dispose();
        _pointSampler?.Dispose();
    }
}