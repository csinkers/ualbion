using System;
using System.Collections.Generic;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public class RenderPassBuilder
{
    readonly string _name;
    readonly RenderManagerBuilder _manager;
    readonly RenderSystemBuilder _system;
    readonly List<string> _dependencies = new();
    readonly List<IRenderer> _renderers = new();
    readonly List<IRenderableSource> _sources = new();
    IResourceProvider _resourceProvider;
    IFramebufferHolder _target;
    RenderPass.RenderMethod _renderFunc;
    RgbaFloat _clearColor = RgbaFloat.Clear;

    RenderPassBuilder(string name, RenderSystemBuilder systemBuilder, RenderManagerBuilder manager)
    {
        _name = name;
        _system = systemBuilder ?? throw new ArgumentNullException(nameof(systemBuilder));
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
    }

    public static RenderPassBuilder Create(string name, RenderSystemBuilder systemBuilder, RenderManagerBuilder manager) 
        => new(name, systemBuilder, manager);

    public RenderPassBuilder Resources(IResourceProvider resourceProvider) { _resourceProvider = resourceProvider; return this; }
    public RenderPassBuilder Target(string name) { _target = _system.GetFramebuffer(name); return this; }

    public RenderPassBuilder Renderer(string name) { _renderers.Add(_manager.GetRenderer(name)); return this; }
    public RenderPassBuilder Renderers(params string[] names)
    {
        foreach(var name in names)
            _renderers.Add(_manager.GetRenderer(name));
        return this;
    }

    public RenderPassBuilder Source(string name) { _sources.Add(_manager.GetSource(name)); return this; }
    public RenderPassBuilder Sources(params string[] names)
    {
        foreach(var name in names)
            _sources.Add(_manager.GetSource(name));

        return this;
    }
    public RenderPassBuilder Dependency(string name) { _dependencies.Add(name); return this; }
    public RenderPassBuilder Render(RenderPass.RenderMethod renderFunc) { _renderFunc = renderFunc; return this; }
    public RenderPassBuilder ClearColor(RgbaFloat clearColor) { _clearColor = clearColor; return this; }

    public RenderPass Build() =>
        new()
        {
            Name = _name,
            RenderFunc = _renderFunc ?? RenderPass.DefaultRender,
            Target = _target ?? throw new InvalidOperationException("A target framebuffer must be supplied"),
            ResourceProvider = _resourceProvider,
            Dependencies = _dependencies.ToArray(),
            Renderers = _renderers,
            Sources = _sources,
            ClearColor = _clearColor
        };
}