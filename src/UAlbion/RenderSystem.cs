using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Etm;
using UAlbion.Core.Veldrid.Meshes;
using UAlbion.Core.Veldrid.Skybox;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Visual;
using UAlbion.Game.Veldrid.Visual;
using Veldrid;
using Veldrid.Utilities;

namespace UAlbion;

#pragma warning disable CA2000
#pragma warning disable CA2213
class RenderSystem : Container, IRenderSystem, IDisposable
{
    readonly DisposeCollector _globalDisposer = new();
    readonly GlobalResourceSetManager _globalSetManager;
    readonly List<IRenderPass<GlobalSet>> _renderPasses = new();
    readonly List<IRenderer<GlobalSet, MainPassSet>> _renderers = new();
    readonly List<IRenderableSource> _sources = new();
    readonly DebugGuiRenderer<DummyResourceSet> _debugRenderer;
    readonly DebugGuiRenderable _debugRenderSource;
    readonly MainFramebuffer _displayBuffer;
    readonly List<string> _cachedStrings = new(); // Avoid per-frame allocations when registering profiling events
    MainRenderPass _mainPass;
    DiagRenderPass _diagPass;
    bool _isDiag = false;

    public FramebufferHolder GameBuffer { get; private set; }

    public RenderSystem() : base("RenderSystem")
    {
        var outputFormat = new OutputDescription(
            new OutputAttachmentDescription(PixelFormat.R32_Float),
            new OutputAttachmentDescription(PixelFormat.B8_G8_R8_A8_UNorm));

        _displayBuffer = new MainFramebuffer();
        _globalSetManager = AttachChild(new GlobalResourceSetManager());
        _globalDisposer.Add(AttachChild(_displayBuffer));

        _renderers.Add(AttachChild(new SpriteRenderer(outputFormat)));
        _renderers.Add(AttachChild(new BlendedSpriteRenderer(outputFormat)));
        _renderers.Add(AttachChild(new TileRenderer(outputFormat)));
        _renderers.Add(AttachChild(new EtmRenderer(outputFormat)));
        _renderers.Add(AttachChild(new MeshRenderer(outputFormat)));
        _renderers.Add(AttachChild(new SkyboxRenderer(outputFormat)));

        foreach (var disposable in _renderers.OfType<IDisposable>())
            _globalDisposer.Add(disposable);

        _sources.Add(AttachChild(new SkyboxManager()));
        _sources.Add(AttachChild(new EtmManager()));
        _sources.Add(AttachChild(new BatchManager<SpriteKey, SpriteInfo>(static (key, f) => f.CreateSpriteBatch(key))));
        _sources.Add(AttachChild(new BatchManager<SpriteKey, BlendedSpriteInfo>(static (key, f) => f.CreateBlendedSpriteBatch(key))));
        _sources.Add(AttachChild(new BatchManager<MeshId, GpuMeshInstanceData>(static (key, f) => ((VeldridCoreFactory)f).CreateMeshBatch(key))));
        _sources.Add(AttachChild(new TileRenderableManager()));

        foreach (var disposable in _sources.OfType<IDisposable>())
            _globalDisposer.Add(disposable);

        AttachChild(new MeshManager());

        _debugRenderer     = AttachChild(new DebugGuiRenderer<DummyResourceSet>(outputFormat));
        _debugRenderSource = AttachChild(new DebugGuiRenderable());
        _globalDisposer.Add(_debugRenderer);


        On<ToggleDiagnosticsEvent>(_ => { _isDiag = !_isDiag; Rebuild(); });
    }

    protected override void Subscribed() => Rebuild();

    void Rebuild()
    {
        if (GameBuffer != null) { GameBuffer.Dispose(); Remove(GameBuffer); GameBuffer = null; }
        if (_mainPass != null) { _mainPass.Dispose(); Remove(_mainPass); _mainPass = null; }
        if (_diagPass != null) { Remove(_diagPass); _diagPass = null; }

        _renderPasses.Clear();
        _cachedStrings.Clear();

        if (_isDiag)
        {
            GameBuffer = new SimpleFramebuffer(64, 64, "FB_Game");
            _diagPass = new DiagRenderPass(_displayBuffer, _debugRenderer, _debugRenderSource);
            Add(GameBuffer);
        }

        _mainPass = new MainRenderPass(_isDiag ? GameBuffer : _displayBuffer);

        foreach (var renderer in _renderers)
            _mainPass.AddRenderer(renderer);

        foreach (var source in _sources)
            _mainPass.AddSource(source);

        Add(_mainPass);
        _renderPasses.Add(_mainPass);

        if (_diagPass != null)
            _renderPasses.Add(_diagPass);
    }

    public void Render(GraphicsDevice graphicsDevice, CommandList frameCommands, FenceHolder fence)
    {
        if (graphicsDevice == null) throw new ArgumentNullException(nameof(graphicsDevice));
        if (frameCommands == null) throw new ArgumentNullException(nameof(frameCommands));
        if (fence == null) throw new ArgumentNullException(nameof(fence));

        int i = 0;
        foreach (var phase in _renderPasses)
        {
            using (FrameEventCached(ref i, phase, (x, n) => $"6.3.{n} Render - {x.Name}"))
            {
                frameCommands.Begin();
                phase.Render(graphicsDevice, frameCommands, _globalSetManager.GlobalSet);
                frameCommands.End();
            }

            fence.Fence.Reset();
            using (FrameEventCached(ref i, phase, (x, n) => $"6.3.{n} Submit commands - {x.Name}"))
                graphicsDevice.SubmitCommands(frameCommands, fence.Fence);

            using (FrameEventCached(ref i, phase, (x, n) => $"6.3.{n} Complete - {x.Name}"))
                graphicsDevice.WaitForFence(fence.Fence);
        }
    }

    public void Dispose()
    {
        _mainPass.Dispose();
        _debugRenderer.Dispose();
        _globalDisposer.DisposeAll();
        GameBuffer?.Dispose();
    }

    FrameTimeTracker FrameEventCached<T>(ref int num, T context, Func<T, int, string> builder)
    {
        if (_cachedStrings.Count <= num)
            _cachedStrings.Add(builder(context, num));

        var message = _cachedStrings[num++];
        return PerfTracker.FrameEvent(message);
    }
}