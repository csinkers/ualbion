using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Veldrid;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion;

public sealed class CopyRenderPass : Component, IRenderPass<GlobalSet>, IDisposable
{

    readonly FullscreenQuadRenderer<DummyResourceSet> _quadRenderer;
    readonly FullscreenQuad _quad;

    public CopyRenderPass(ITextureHolder source, IFramebufferHolder dest)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (dest?.OutputDescription == null) throw new ArgumentNullException(nameof(dest));

        Framebuffer = dest;
        _quadRenderer = new FullscreenQuadRenderer<DummyResourceSet>();
        _quad = new FullscreenQuad("Quad", DrawLayer.Compositing,
            source,
            new Vector4(0, 0, 1, 1),
            dest.OutputDescription.Value);

        AttachChild(_quadRenderer);
        AttachChild(_quad);
    }

    public string Name => "Copy";
    public IFramebufferHolder Framebuffer { get; }
    public Vector4 NormalisedDestRectangle
    {
        get => _quad.NormalisedDestRectangle;
        set => _quad.NormalisedDestRectangle = value;
    }

    public void Render(GraphicsDevice device, CommandList cl, GlobalSet globalSet)
    {
        if (cl == null) throw new ArgumentNullException(nameof(cl));
        cl.SetFramebuffer(Framebuffer.Framebuffer);
        cl.SetFullViewports();
        cl.SetFullScissorRects();
        cl.ClearColorTarget(0, new RgbaFloat(0, 0, 0, 1.0f));
        _quadRenderer.Render(_quad, cl, device, globalSet, DummyResourceSet.Instance);
    }

    public void Dispose()
    {
        _quadRenderer?.Dispose();
        _quad?.Dispose();
    }
}