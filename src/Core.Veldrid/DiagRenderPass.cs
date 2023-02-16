using System;
using UAlbion.Api.Eventing;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public sealed class DiagRenderPass : Component, IRenderPass<GlobalSet>
{
    readonly DebugGuiRenderer<DummyResourceSet> _renderer;
    readonly DebugGuiRenderable _renderable;

    public DiagRenderPass(IFramebufferHolder framebuffer, DebugGuiRenderer<DummyResourceSet> renderer, DebugGuiRenderable source)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _renderable = source ?? throw new ArgumentNullException(nameof(source));
        Framebuffer = framebuffer ?? throw new ArgumentNullException(nameof(framebuffer));
    }

    public string Name => "Diag";
    public override string ToString() => $"Pass:{Name}";
    public IFramebufferHolder Framebuffer { get; }
    public void Render(GraphicsDevice device, CommandList cl, GlobalSet globalSet)
    {
        if (device == null) throw new ArgumentNullException(nameof(device));
        if (cl == null) throw new ArgumentNullException(nameof(cl));

        cl.SetFramebuffer(Framebuffer.Framebuffer);
        cl.SetFullViewports();
        cl.SetFullScissorRects();
        cl.ClearColorTarget(0, new RgbaFloat(0, 0, 0, 1.0f));
        _renderer.Render(_renderable, cl, device, globalSet, DummyResourceSet.Instance);
    }
}