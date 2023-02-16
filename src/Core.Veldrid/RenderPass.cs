using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public sealed class MainRenderPass : Component, IRenderPass<GlobalSet>, IDisposable
{
    readonly Dictionary<Type, IRenderer<GlobalSet, MainPassSet>> _rendererLookup = new();
    readonly List<IRenderer<GlobalSet, MainPassSet>> _renderers = new();
    readonly List<IRenderable> _renderList = new();
    readonly List<IRenderableSource> _sources = new();
    readonly SingleBuffer<CameraUniform> _camera;
    readonly MainPassSet _passSet;
    (float Red, float Green, float Blue, float Alpha) _clearColour;

    public string Name => "Main";
    public IFramebufferHolder Framebuffer { get; }
    public override string ToString() => $"Pass:{Name}";

    public MainRenderPass(IFramebufferHolder framebuffer)
    {
        Framebuffer = framebuffer ?? throw new ArgumentNullException(nameof(framebuffer));

        On<SetClearColourEvent>(e => _clearColour = (e.Red, e.Green, e.Blue, e.Alpha));
        On<PrepareFrameEvent>(_ => UpdatePerFrameResources());

        _camera = new SingleBuffer<CameraUniform>(BufferUsage.UniformBuffer | BufferUsage.Dynamic, "B_Camera");
        _passSet = new MainPassSet
        {
            Name = "RS_MainPass",
            Camera = _camera,
        };

        AttachChild(_camera);
        AttachChild(_passSet);
    }

    public void AddRenderer(IRenderer<GlobalSet, MainPassSet> renderer)
    {
        if (renderer == null) throw new ArgumentNullException(nameof(renderer));

        var types = renderer.HandledTypes;
        if (types == null || types.Length == 0)
            return;

        if (!_renderers.Contains(renderer))
            _renderers.Add(renderer);

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
    }

    public void AddSource(IRenderableSource source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        _sources.Add(source);
    }

    public void Render(GraphicsDevice device, CommandList cl, GlobalSet globalSet)
    {
        if (device == null) throw new ArgumentNullException(nameof(device));
        if (cl == null) throw new ArgumentNullException(nameof(cl));

        // Sort:
        // Opaque, front-to-back (map, then sprites)
        // Transparent, back-to-front (1 call per map-tile / sprite)
        // CoreTrace.Log.Info("Scene", "Sorted processed renderables");

        // Main scene
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
            if (_rendererLookup.TryGetValue(renderable.GetType(), out var renderer))
                renderer.Render(renderable, cl, device, globalSet, _passSet);
    }

    void UpdatePerFrameResources()
    {
        var camera = Resolve<ICamera>();
        camera.Viewport = new Vector2(Framebuffer.Width, Framebuffer.Height);

        _camera.Data = new CameraUniform
        {
            WorldSpacePosition = camera.Position,
            CameraDirection = new Vector2(camera.Pitch, camera.Yaw),
            Resolution =  new Vector2(Framebuffer.Width, Framebuffer.Height),
            Projection = camera.ProjectionMatrix,
            View = camera.ViewMatrix,
        };
    }

    public void Dispose()
    {
        _camera?.Dispose();
        _passSet?.Dispose();
    }
}