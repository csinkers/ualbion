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
    public RenderManagerBuilder Framebuffer(string name, IFramebufferHolder framebuffer) { _framebuffers.Add(name, framebuffer); return this; }

    RenderManagerBuilder() { }
    public static RenderManagerBuilder Create() => new();

    public RenderManagerBuilder Renderer(string name, IRenderer renderer) { _renderers.Add(name, renderer); return this; }
    public RenderManagerBuilder Source(string name, IRenderableSource source) { _sources.Add(name, source); return this; }
    public RenderManagerBuilder System(string name, Func<RenderSystemBuilder, RenderSystem> build)
    {
        _pipelines.Add(name, build(RenderSystemBuilder.Create(name, this)));
        return this;
    }

    internal IRenderer GetRenderer(string name) => _renderers[name];
    internal IRenderableSource GetSource(string name) => _sources[name];
    internal IFramebufferHolder GetFramebuffer(string name) => _framebuffers[name];

    public RenderManager Build() =>
        new()
        {
            Renderers = _renderers.Values.ToList(),
            Sources = _sources.Values.ToList(),
            Framebuffers = _framebuffers.Values.ToList(),
            Systems = _pipelines,
        };
}