using System;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public sealed class GlobalResourceSetProvider : Component, IResourceProvider, IDisposable
{
    readonly SingleBuffer<GlobalInfo> _globalInfo;
    readonly SamplerHolder _paletteSampler;
    readonly GlobalSet _globalSet;
    ITextureHolder _dayPalette;
    ITextureHolder _nightPalette;

    public IResourceSetHolder ResourceSet => _globalSet;

    public GlobalResourceSetProvider()
    {
        _paletteSampler = AttachChild(new SamplerHolder
        {
            AddressModeU = SamplerAddressMode.Clamp,
            AddressModeV = SamplerAddressMode.Clamp,
            AddressModeW = SamplerAddressMode.Clamp,
            BorderColor = SamplerBorderColor.TransparentBlack,
            Filter = SamplerFilter.MinPoint_MagPoint_MipPoint,
        });

        _globalInfo = AttachChild(new SingleBuffer<GlobalInfo>(BufferUsage.UniformBuffer | BufferUsage.Dynamic, "B_Global"));
        _globalSet = AttachChild(new GlobalSet
        {
            Name = "RS_Global",
            Global = _globalInfo,
            Sampler = _paletteSampler,
        });

        On<PrepareFrameEvent>(_ => UpdatePerFrameResources());
    }

    void UpdatePerFrameResources()
    {
        var clock = TryResolve<IClock>();
        var textureSource = Resolve<ITextureSource>();
        var paletteManager = Resolve<IPaletteManager>();
        var engineFlags = Var(CoreVars.User.EngineFlags);

        var dayPalette = textureSource.GetSimpleTexture(paletteManager.Day.Texture);
        var nightTexture = paletteManager.Night?.Texture ?? paletteManager.Day.Texture;
        var nightPalette = textureSource.GetSimpleTexture(nightTexture);

        if (_dayPalette != dayPalette)
        {
            _dayPalette = dayPalette;
            _globalSet.DayPalette = dayPalette;
        }

        if (_nightPalette != nightPalette)
        {
            _nightPalette = nightPalette;
            _globalSet.NightPalette = nightPalette;
        }

        var info = new GlobalInfo
        {
            Time = clock?.ElapsedTime ?? 0,
            EngineFlags = engineFlags,
            PaletteBlend = paletteManager.Blend,
            PaletteFrame = paletteManager.Frame,
        };

        _globalInfo.Data = info;
    }

    public void Dispose()
    {
        _globalInfo?.Dispose();
        _paletteSampler?.Dispose();
        _globalSet?.Dispose();
    }

}