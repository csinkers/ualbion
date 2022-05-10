﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public sealed class RenderPass : Component, IRenderPass, IDisposable
{
    readonly Dictionary<Type, IRenderer> _rendererLookup = new();
    readonly List<IRenderer> _renderers = new();
    readonly List<IRenderable> _renderList = new();
    readonly List<IRenderableSource> _sources = new();
    readonly SingleBuffer<GlobalInfo> _globalInfo;
    readonly SingleBuffer<ProjectionMatrix> _projection;
    readonly SingleBuffer<ViewMatrix> _view;
    readonly SamplerHolder _paletteSampler;
    readonly CommonSet _commonSet;
    ITextureHolder _dayPalette;
    ITextureHolder _nightPalette;
    (float Red, float Green, float Blue, float Alpha) _clearColour;

    public RenderPass(string name, IFramebufferHolder framebuffer)
    {
        Framebuffer = framebuffer ?? throw new ArgumentNullException(nameof(framebuffer));
        Name = name;

        On<SetClearColourEvent>(e => _clearColour = (e.Red, e.Green, e.Blue, e.Alpha));
        On<RenderEvent>(_ => UpdatePerFrameResources());

        _projection = new SingleBuffer<ProjectionMatrix>(BufferUsage.UniformBuffer | BufferUsage.Dynamic, "M_Projection");
        _view = new SingleBuffer<ViewMatrix>(BufferUsage.UniformBuffer | BufferUsage.Dynamic, "M_View");
        _globalInfo = new SingleBuffer<GlobalInfo>(BufferUsage.UniformBuffer | BufferUsage.Dynamic, "B_GlobalInfo");
        _paletteSampler = new SamplerHolder
            {
                AddressModeU = SamplerAddressMode.Clamp,
                AddressModeV = SamplerAddressMode.Clamp,
                AddressModeW = SamplerAddressMode.Clamp,
                BorderColor = SamplerBorderColor.TransparentBlack,
                Filter = SamplerFilter.MinPoint_MagPoint_MipPoint,
            };

        _commonSet = new CommonSet
        {
            Name = "RS_Common",
            GlobalInfo = _globalInfo,
            Projection = _projection,
            View = _view,
            Sampler = _paletteSampler,
        };
        AttachChild(_projection);
        AttachChild(_view);
        AttachChild(_globalInfo);
        AttachChild(_paletteSampler);
        AttachChild(_commonSet);
    }

    public RenderPass AddRenderer(IRenderer renderer)
    {
        if (renderer == null) throw new ArgumentNullException(nameof(renderer));

        var types = renderer.HandledTypes;
        if (types == null || types.Length == 0)
            return this;

        if (!_renderers.Contains(renderer))
        {
            _renderers.Add(renderer);
            AttachChild(renderer);
        }

        foreach (var type in types)
        {
            if (!_rendererLookup.TryAdd(type, renderer))
            {
                throw new InvalidOperationException(
                    $"Tried to register renderer of type {renderer.GetType().Name} for" +
                    $" rendering \"{type.Name}\", but they are already being handled by " +
                    _rendererLookup[type].GetType().Name);
            }
        }

        return this;
    }

    public RenderPass AddSource(IRenderableSource source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        _sources.Add(source);
        return this;
    }

    public string Name { get; }
    public IFramebufferHolder Framebuffer { get; }
    public override string ToString() => $"Scene:{Name}";
    public void Render(GraphicsDevice device, CommandList cl)
    {
        if (device == null) throw new ArgumentNullException(nameof(device));
        if (cl == null) throw new ArgumentNullException(nameof(cl));

        // Sort:
        // Opaque, front-to-back (map, then sprites)
        // Transparent, back-to-front (1 call per map-tile / sprite)
        // CoreTrace.Log.Info("Scene", "Sorted processed renderables");

        // Main scene
        using (PerfTracker.FrameEvent("6.2.3 Main scene pass"))
        {
            cl.SetFramebuffer(Framebuffer.Framebuffer);
            cl.SetFullViewports();
            cl.SetFullScissorRects();
            cl.ClearColorTarget(0, new RgbaFloat(_clearColour.Red, _clearColour.Green, _clearColour.Blue, _clearColour.Alpha));
            cl.ClearDepthStencil(device.IsDepthRangeZeroToOne ? 1f : 0f);

            _renderList.Clear();
            foreach (var source in _sources)
                source.Collect(_renderList);

            _renderList.Sort((x, y) =>
            {
                var x2 = (ushort)x.RenderOrder;
                var y2 = (ushort)y.RenderOrder;
                return x2 < y2 ? -1 : x2 > y2 ? 1 : 0;
            });

            foreach (var renderable in _renderList)
            {
                if (_rendererLookup.TryGetValue(renderable.GetType(), out var renderer))
                    renderer.Render(renderable, _commonSet, Framebuffer, cl, device);
            }
        }
    }

    void UpdatePerFrameResources()
    {
        var camera = Resolve<ICamera>();
        var clock = TryResolve<IClock>();
        var settings = TryResolve<IEngineSettings>();
        var paletteManager = Resolve<IPaletteManager>();
        var textureSource = Resolve<ITextureSource>();

        camera.Viewport = new Vector2(Framebuffer.Width, Framebuffer.Height);
        var dayPalette = textureSource.GetSimpleTexture(paletteManager.Day.Texture);
        var nightTexture = paletteManager.Night?.Texture ?? paletteManager.Day.Texture;
        var nightPalette = textureSource.GetSimpleTexture(nightTexture);

        if (_dayPalette != dayPalette)
        {
            _dayPalette = dayPalette;
            _commonSet.DayPalette = dayPalette;
        }

        if (_nightPalette != nightPalette)
        {
            _nightPalette = nightPalette;
            _commonSet.NightPalette = nightPalette;
        }

        var info = new GlobalInfo
        {
            WorldSpacePosition = camera.Position,
            CameraDirection = new Vector2(camera.Pitch, camera.Yaw),
            Resolution =  new Vector2(Framebuffer.Width, Framebuffer.Height),
            Time = clock?.ElapsedTime ?? 0,
            EngineFlags = settings?.Flags ?? 0,
            PaletteBlend = paletteManager.Blend,
            PaletteFrame = paletteManager.Frame
        };

        _projection.Data = new ProjectionMatrix(camera.ProjectionMatrix);
        _view.Data = new ViewMatrix(camera.ViewMatrix);
        _globalInfo.Data = info;
    }

    public void Dispose()
    {
        _commonSet?.Dispose();
        _paletteSampler.Dispose();
        _globalInfo.Dispose();
        _projection.Dispose();
        _view.Dispose();
        foreach (var renderer in _renderers.OfType<IDisposable>())
            renderer.Dispose();
        _renderers.Clear();
        _rendererLookup.Clear();
    }
}