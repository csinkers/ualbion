using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Eventing;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public class RenderSystemBuilder
{
    readonly string _name;
    readonly RenderManagerBuilder _manager;
    readonly Dictionary<string, RenderPass> _passes = new();
    readonly Dictionary<string, IComponent> _components = new();
    readonly Dictionary<string, IFramebufferHolder> _framebuffers = new();
    IResourceProvider _resourceProvider;
    Action<RenderSystem, GraphicsDevice> _preRender;
    Action<RenderSystem, GraphicsDevice> _postRender;
    bool _built;

    RenderSystemBuilder(string name, RenderManagerBuilder manager)
    {
        _name = name;
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
    }

    public static RenderSystemBuilder Create(string name, RenderManagerBuilder manager) => new(name, manager);
    public RenderSystemBuilder PreRender(Action<RenderSystem, GraphicsDevice> method) { Check(); _preRender = method; return this; }
    public RenderSystemBuilder PostRender(Action<RenderSystem, GraphicsDevice> method) { Check(); _postRender = method; return this; }
    public RenderSystemBuilder Component(string name, IComponent component) { Check(); _components.Add(name, component); return this; }
    public RenderSystemBuilder Resources(IResourceProvider resourceProvider) { Check(); _resourceProvider = resourceProvider; return this; }
    public RenderSystemBuilder Framebuffer(string name, IFramebufferHolder framebuffer) { Check(); _framebuffers.Add(name, framebuffer); return this; }
    public IComponent GetComponent(string name) { Check(); return _components[name]; }
    public IRenderer GetRenderer(string name) => _manager.GetRenderer(name);

    public IFramebufferHolder GetFramebuffer(string name)
    {
        Check();
        return _framebuffers.TryGetValue(name, out var framebuffer)
            ? framebuffer
            : _manager.GetFramebuffer(name);
    }

    public RenderSystemBuilder Pass(string name, Func<RenderPassBuilder, RenderPass> builderFunc)
    {
        Check();
        _passes.Add(name, builderFunc(RenderPassBuilder.Create(name, this, _manager)));
        return this;
    }

    public RenderSystem Build()
    {
        Check();
        _built = true;
        return new(_components.Values)
        {
            Name = _name,
            ResourceProvider = _resourceProvider,
            Framebuffers = _framebuffers.Select(x => x.Value).ToList(),
            Passes = GetTopogicalOrder(),
            PreRender = _preRender,
            PostRender = _postRender,
        };
    }

    List<RenderPass> GetTopogicalOrder()
    {
        int i = 0;
        bool found = true;
        var ordering = _passes.ToDictionary(x => x.Key, _ => (int?)null);
        while (found)
        {
            found = false;
            foreach (var (key, pass) in _passes)
            {
                if (ordering[key].HasValue)
                    continue;

                if (pass.Dependencies != null && pass.Dependencies.Any(x => !ordering[x].HasValue))
                    continue; // If we haven't visited all of the parents we can't evaluate this node yet.

                ordering[key] = i++;
                found = true;
                break;
            }
        }

        return ordering.OrderBy(x => x.Value).Select(x => _passes[x.Key]).ToList();
    }

    void Check()
    {
        if (_built)
            throw new InvalidOperationException($"Tried to access a {nameof(RenderSystemBuilder)} for {_name}, but it has already been built!");
    }
}