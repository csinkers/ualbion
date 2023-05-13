using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Core.Visual;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public class RenderManagerBuilder
{
    readonly Dictionary<string, IRenderer> _renderers = new();
    readonly Dictionary<string, IRenderableSource> _sources = new();
    readonly Dictionary<string, RenderSystem> _pipelines = new();
    readonly Dictionary<string, IFramebufferHolder> _framebuffers = new();
    bool _built;

    public RenderManagerBuilder Framebuffer(string name, IFramebufferHolder framebuffer) { Check(); _framebuffers.Add(name, framebuffer); return this; }

    RenderManagerBuilder() { }
    public static RenderManagerBuilder Create() => new();

    public RenderManagerBuilder Renderer(string name, IRenderer renderer) { Check(); _renderers.Add(name, renderer); return this; }
    public RenderManagerBuilder Source(string name, IRenderableSource source) { Check(); _sources.Add(name, source); return this; }
    public RenderManagerBuilder System(string name, Func<RenderSystemBuilder, RenderSystem> build)
    {
        if (build == null) throw new ArgumentNullException(nameof(build));

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
            Renderers = _renderers.Values.ToList(),
            Sources = _sources.Values.ToList(),
            Framebuffers = _framebuffers.Values.ToList(),
            Systems = _pipelines,
        };
    }

    void Check()
    {
        if (_built)
            throw new InvalidOperationException($"Tried to access a {nameof(RenderManagerBuilder)}, but it has already been built!");
    }
}