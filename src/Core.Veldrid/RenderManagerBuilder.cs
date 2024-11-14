using System;
using System.Collections.Generic;
using UAlbion.Core.Visual;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public class RenderManagerBuilder
{
    readonly Dictionary<string, IRenderer> _renderers = [];
    readonly Dictionary<string, IRenderableSource> _sources = [];
    readonly Dictionary<string, RenderSystem> _pipelines = [];
    readonly Dictionary<string, IFramebufferHolder> _framebuffers = [];
    bool _built;

    public RenderManagerBuilder Framebuffer(string name, IFramebufferHolder framebuffer) { Check(); _framebuffers.Add(name, framebuffer); return this; }

    RenderManagerBuilder() { }
    public static RenderManagerBuilder Create() => new();

    public RenderManagerBuilder Renderer(string name, IRenderer renderer) { Check(); _renderers.Add(name, renderer); return this; }
    public RenderManagerBuilder Source(string name, IRenderableSource source) { Check(); _sources.Add(name, source); return this; }
    public RenderManagerBuilder System(string name, Func<RenderSystemBuilder, RenderSystem> build)
    {
        ArgumentNullException.ThrowIfNull(build);

        Check();
        _pipelines.Add(name, build(RenderSystemBuilder.Create(name, this)));
        return this;
    }

    internal IRenderer GetRenderer(string name) { Check(); return _renderers[name]; } 
    internal IRenderableSource GetSource(string name) { Check(); return _sources[name]; } 
    internal IFramebufferHolder GetFramebuffer(string name) { Check(); return _framebuffers[name]; } 
    public RenderManager Build()
    {
        Check();
        _built = true;
        return new()
        {
            Renderers = _renderers,
            Sources = _sources,
            Framebuffers = _framebuffers,
            Systems = _pipelines,
        };
    }

    void Check()
    {
        if (_built)
            throw new InvalidOperationException($"Tried to access a {nameof(RenderManagerBuilder)}, but it has already been built!");
    }
}