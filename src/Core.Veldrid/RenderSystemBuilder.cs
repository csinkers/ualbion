using System;
using System.Collections.Generic;
using System.Linq;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public class RenderSystemBuilder
{
    readonly string _name;
    readonly RenderManagerBuilder _manager;
    readonly Dictionary<string, RenderPass> _passes = new();
    readonly Dictionary<string, IFramebufferHolder> _framebuffers = new();
    IResourceProvider _resourceProvider;
    Action<RenderSystem> _preRender;
    Action<RenderSystem> _postRender;

    RenderSystemBuilder(string name, RenderManagerBuilder manager)
    {
        _name = name;
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
    }

    public static RenderSystemBuilder Create(string name, RenderManagerBuilder manager) => new(name, manager);
    public RenderSystemBuilder PreRender(Action<RenderSystem> method) { _preRender = method; return this; }
    public RenderSystemBuilder PostRender(Action<RenderSystem> method) { _postRender = method; return this; }
    public RenderSystemBuilder Resources(IResourceProvider resourceProvider) { _resourceProvider = resourceProvider; return this; }
    public RenderSystemBuilder Framebuffer(string name, IFramebufferHolder framebuffer) { _framebuffers.Add(name, framebuffer); return this; }
    public IFramebufferHolder GetFramebuffer(string name) 
        => _framebuffers.TryGetValue(name, out var framebuffer) 
            ? framebuffer 
            : _manager.GetFramebuffer(name);

    public RenderSystemBuilder Pass(string name, Func<RenderPassBuilder, RenderPass> builderFunc)
    {
        _passes.Add(name, builderFunc(RenderPassBuilder.Create(name, this, _manager)));
        return this;
    }

    public RenderSystem Build() =>
        new()
        {
            Name = _name,
            ResourceProvider = _resourceProvider,
            Framebuffers = _framebuffers.Select(x => x.Value).ToList(),
            Passes = GetTopogicalOrder(),
            PreRender = _preRender,
            PostRender = _postRender,
        };

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
}