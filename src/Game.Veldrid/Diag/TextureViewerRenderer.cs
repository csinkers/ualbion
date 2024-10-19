using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Game.Veldrid.Diag;

public class TextureViewerRenderer : Component, ICameraProvider
{
    public uint Width => _fb.Width;
    public uint Height => _fb.Height;

    public int Zoom
    {
        get => _zoom;
        set
        {
            if (_zoom == value)
                return;

            _zoom = value;
            int factor = (int)Math.Pow(2, _zoom);
            _fb.Width = (uint)(_defaultWidth * factor);
            _fb.Height = (uint)(_defaultHeight * factor);
        }
    }

    public int Frame
    {
        get => _sprite.Frame;
        set => _sprite.Frame = value;
    }

    public int FrameCount => _sprite.FrameCount;

    public ITextureHolder Palette
    {
        get => _globalSet.DayPalette;
        set
        {
            _globalSet.DayPalette = value;
            _globalSet.NightPalette = value;
        }
    }

    readonly List<IRenderable> _renderables = new();
    readonly SimpleFramebuffer _fb;
    readonly CommandListHolder _cl;
    readonly FenceHolder _fence;
    readonly BatchManager<SpriteKey, SpriteInfo> _batchManager;
    readonly GlobalResourceSetProvider _globalSet;
    readonly MainPassResourceProvider _passSet;
    readonly OrthographicCamera _camera;
    readonly Sprite _sprite;
    readonly int _defaultWidth;
    readonly int _defaultHeight;
    int _zoom;

    public TextureViewerRenderer(ITexture texture)
    {
        _cl = AttachChild(new CommandListHolder("cl_texViewer"));
        _fence = AttachChild(new FenceHolder("f_texViewer"));
        _batchManager = AttachChild(new BatchManager<SpriteKey, SpriteInfo>(static (key, f) => f.CreateSpriteBatch(key), false));
        _globalSet = AttachChild(new GlobalResourceSetProvider("TexViewerGlobal"));
        _camera = AttachChild(new OrthographicCamera());
        _sprite = AttachChild(new Sprite(
            AssetId.None,
            DrawLayer.Interface,
            SpriteKeyFlags.UsePalette,
            0, // SpriteFlags.FlipVertical,
            _ => texture, _batchManager));

        foreach (var region in texture.Regions)
        {
            if (region.Width > _defaultWidth) _defaultWidth = region.Width;
            if (region.Height > _defaultHeight) _defaultHeight = region.Height;
        }

        _fb = AttachChild(new SimpleFramebuffer("fb_texViewer", (uint)_defaultWidth, (uint)_defaultHeight));
        _passSet = AttachChild(new MainPassResourceProvider(_fb, this));

        On<ImGuiPreRenderEvent>(e => Render(e.Device));
    }

    void Render(GraphicsDevice gd)
    {
        var cl = _cl.CommandList;
        if (cl == null)
            return;

        _renderables.Clear();
        cl.Begin();

        cl.SetFramebuffer(_fb.Framebuffer);
        cl.SetFullViewports();
        cl.SetFullScissorRects();
        cl.ClearColorTarget(0, RgbaFloat.Black);
        cl.ClearDepthStencil(gd.IsDepthRangeZeroToOne ? 1f : 0f);

        var renderer = Resolve<IRenderManager>().GetRenderer(AlbionRenderSystemConstants.R_Sprite);
        _batchManager.Collect(_renderables);

        foreach (var renderable in _renderables)
            renderer.Render(renderable, _cl.CommandList, gd, _globalSet.ResourceSet, _passSet.ResourceSet);

        _cl.CommandList.End();

        gd.SubmitCommands(_cl.CommandList, _fence.Fence);
        gd.WaitForFence(_fence.Fence); // Slow but simple
    }

    public ICamera Camera => _camera;
    public Texture Framebuffer => _fb.Color.DeviceTexture;
}