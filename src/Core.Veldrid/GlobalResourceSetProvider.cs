using System;
using UAlbion.Api.Eventing;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public sealed class GlobalResourceSetProvider : Component, IResourceProvider, IDisposable
{
    readonly SingleBuffer<GlobalInfo> _globalInfo;
    readonly SamplerHolder _paletteSampler;
    readonly GlobalSet _globalSet;

    public IResourceSetHolder ResourceSet => _globalSet;

    public GlobalResourceSetProvider(string name = "Global")
    {
        _paletteSampler = new SamplerHolder
        {
            AddressModeU = SamplerAddressMode.Clamp,
            AddressModeV = SamplerAddressMode.Clamp,
            AddressModeW = SamplerAddressMode.Clamp,
            BorderColor = SamplerBorderColor.TransparentBlack,
            Filter = SamplerFilter.MinPoint_MagPoint_MipPoint,
        };

        _globalInfo = new SingleBuffer<GlobalInfo>(BufferUsage.UniformBuffer | BufferUsage.Dynamic, $"B_{name}");
        _globalSet = new GlobalSet
        {
            Name = $"RS_{name}",
            Global = _globalInfo,
            Sampler = _paletteSampler,
        };

        AttachChild(_paletteSampler);
        AttachChild(_globalInfo);
        AttachChild(_globalSet);
    }

    public ITextureHolder DayPalette
    {
        get => _globalSet.DayPalette;
        set => _globalSet.DayPalette = value;
    }

    public ITextureHolder NightPalette
    {
        get => _globalSet.NightPalette;
        set => _globalSet.NightPalette = value;
    }

    public GlobalInfo GlobalInfo
    {
        get => _globalInfo.Data;
        set => _globalInfo.Data = value;
    }

    public void Dispose()
    {
        _globalInfo?.Dispose();
        _paletteSampler?.Dispose();
        _globalSet?.Dispose();
    }
}