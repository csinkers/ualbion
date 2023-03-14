using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Core.Visual;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public class RenderSystemBuilder
{
    readonly Dictionary<string, IRenderer> _renderers = new();
    readonly Dictionary<string, IRenderableSource> _sources = new();
    readonly Dictionary<string, RenderPipeline> _pipelines = new();
    readonly Dictionary<string, IFramebufferHolder> _framebuffers = new();
    public RenderSystemBuilder Framebuffer(string name, IFramebufferHolder framebuffer) { _framebuffers.Add(name, framebuffer); return this; }

    RenderSystemBuilder() { }
    public static RenderSystemBuilder Create() => new();

    public RenderSystemBuilder Renderer(string name, IRenderer renderer) { _renderers.Add(name, renderer); return this; }
    public RenderSystemBuilder Source(string name, IRenderableSource source) { _sources.Add(name, source); return this; }
    public RenderSystemBuilder Pipeline(string name, Func<RenderPipelineBuilder, RenderPipeline> build)
    {
        _pipelines.Add(name, build(RenderPipelineBuilder.Create(name, this)));
        return this;
    }

    internal IRenderer GetRenderer(string name) => _renderers[name];
    internal IRenderableSource GetSource(string name) => _sources[name];
    internal IFramebufferHolder GetFramebuffer(string name) => _framebuffers[name];

    public RenderSystem Build() =>
        new()
        {
            Renderers = _renderers.Values.ToList(),
            Sources = _sources.Values.ToList(),
            Framebuffers = _framebuffers.Values.ToList(),
            Pipelines = _pipelines,
        };
}