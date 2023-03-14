using System;
using System.Collections.Generic;
using UAlbion.Core.Visual;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public class RenderPassBuilder
{
    readonly string _name;
    readonly RenderSystemBuilder _system;
    readonly RenderPipelineBuilder _pipeline;
    readonly List<string> _dependencies = new();
    readonly List<IRenderer> _renderers = new();
    readonly List<IRenderableSource> _sources = new();
    IResourceProvider _resourceProvider;
    IFramebufferHolder _target;
    RenderPass.RenderMethod _renderFunc;

    RenderPassBuilder(string name, RenderPipelineBuilder pipelineBuilder, RenderSystemBuilder system)
    {
        _name = name;
        _pipeline = pipelineBuilder ?? throw new ArgumentNullException(nameof(pipelineBuilder));
        _system = system ?? throw new ArgumentNullException(nameof(system));
    }

    public static RenderPassBuilder Create(string name, RenderPipelineBuilder pipelineBuilder, RenderSystemBuilder system) 
        => new(name, pipelineBuilder, system);

    public RenderPassBuilder Resources(IResourceProvider resourceProvider) { _resourceProvider = resourceProvider; return this; }
    public RenderPassBuilder Target(string name) { _target = _pipeline.GetFramebuffer(name); return this; }

    public RenderPassBuilder Renderer(string name) { _renderers.Add(_system.GetRenderer(name)); return this; }
    public RenderPassBuilder Renderers(params string[] names)
    {
        foreach(var name in names)
            _renderers.Add(_system.GetRenderer(name));
        return this;
    }

    public RenderPassBuilder Source(string name) { _sources.Add(_system.GetSource(name)); return this; }
    public RenderPassBuilder Sources(params string[] names)
    {
        foreach(var name in names)
            _sources.Add(_system.GetSource(name));

        return this;
    }
    public RenderPassBuilder Dependency(string name) { _dependencies.Add(name); return this; }
    public RenderPassBuilder Render(RenderPass.RenderMethod renderFunc) { _renderFunc = renderFunc; return this; }

    public RenderPass Build() =>
        new()
        {
            Name = _name,
            RenderFunc = _renderFunc ?? throw new InvalidOperationException("A render function must be supplied"),
            Target = _target ?? throw new InvalidOperationException("A target framebuffer must be supplied"),
            ResourceProvider = _resourceProvider,
            Dependencies = _dependencies.ToArray(),
            Renderers = _renderers,
            Sources = _sources,
        };
}