using System;
using System.Collections.Generic;
using System.Linq;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public class RenderPipelineBuilder
{
    readonly string _name;
    readonly RenderSystemBuilder _system;
    readonly Dictionary<string, RenderPass> _passes = new();
    readonly Dictionary<string, IFramebufferHolder> _framebuffers = new();
    IResourceProvider _resourceProvider;

    RenderPipelineBuilder(string name, RenderSystemBuilder system)
    {
        _name = name;
        _system = system ?? throw new ArgumentNullException(nameof(system));
    }

    public static RenderPipelineBuilder Create(string name, RenderSystemBuilder manager) => new(name, manager);
    public RenderPipelineBuilder Resources(IResourceProvider resourceProvider) { _resourceProvider = resourceProvider; return this; }
    public RenderPipelineBuilder Framebuffer(string name, IFramebufferHolder framebuffer) { _framebuffers.Add(name, framebuffer); return this; }
    public IFramebufferHolder GetFramebuffer(string name) 
        => _framebuffers.TryGetValue(name, out var framebuffer) 
            ? framebuffer 
            : _system.GetFramebuffer(name);

    public RenderPipelineBuilder Pass(string name, Func<RenderPassBuilder, RenderPass> builderFunc)
    {
        _passes.Add(name, builderFunc(RenderPassBuilder.Create(name, this, _system)));
        return this;
    }

    public RenderPipeline Build() =>
        new()
        {
            Name = _name,
            ResourceProvider = _resourceProvider,
            Framebuffers = _framebuffers.Select(x => x.Value).ToList(),
            Passes = GetTopogicalOrder()
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