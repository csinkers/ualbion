using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Diag;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Game.Veldrid.Diag;

public class TextureViewerRenderer2 : Component, ICameraProvider
{
    readonly byte[] _framesBuf = new byte[1024];
    readonly List<IRenderable> _renderables = [];
    readonly SimpleFramebuffer _fb;
    readonly CommandListHolder _cl;
    readonly FenceHolder _fence;
    readonly BatchManager<SpriteKey, SpriteInfo> _batchManager;
    readonly GlobalResourceSetProvider _globalSet;
    readonly MainPassResourceProvider _passSet;
    readonly OrthographicCamera _camera;
    readonly MonsterSprite _sprite;
    int _zoom;

    int[] _frames = [];
    int _frameIndex;
    DateTime _lastTransition = DateTime.UtcNow;
    float _animSpeed = 7.0f;
    bool _isAnimating;
    bool _firstUpdate = true;

    public ICamera Camera => _camera;

    public ITextureHolder Palette
    {
        get => _globalSet.DayPalette;
        set
        {
            _globalSet.DayPalette = value;
            _globalSet.NightPalette = value;
        }
    }

    public TextureViewerRenderer2(ITexture texture)
    {
        _cl = AttachChild(new CommandListHolder("cl_texViewer"));
        _fence = AttachChild(new FenceHolder("f_texViewer"));
        _batchManager = AttachChild(new BatchManager<SpriteKey, SpriteInfo>(static (key, f) => f.CreateSpriteBatch(key), false));
        _globalSet = AttachChild(new GlobalResourceSetProvider("TexViewerGlobal"));
        _camera = AttachChild(new OrthographicCamera());
        _camera.Position = Vector3.Zero;

        _sprite = AttachChild(new MonsterSprite(
            AssetId.None,
            DrawLayer.Billboards,
            SpriteKeyFlags.UsePalette,
            _ => texture,
            _batchManager));

        _sprite.Position = new Vector3(0, 0, 1.0f);

        _fb = AttachChild(new SimpleFramebuffer("fb_texViewer", (uint)_sprite.MaxSize.X, (uint)_sprite.MaxSize.Y));
        _passSet = AttachChild(new MainPassResourceProvider(_fb, this));

        On<ImGuiPreRenderEvent>(e => Render(e.Device));
    }

    public void Draw()
    {
        if (ImGui.SliderInt("Zoom", ref _zoom, 0, 4) || _firstUpdate)
        {
            int zoomFactor = (int)Math.Pow(2, _zoom);
            _sprite.Scale = new Vector2(zoomFactor, zoomFactor);
            _camera.Position = new Vector3(0, -_sprite.MaxSize.Y / 2, 0);
            _firstUpdate = false;
        }

        ImGui.Text($"Max Frame: {_sprite.FrameCount}");

        if (ImGui.InputText("Frames", _framesBuf, (uint)_framesBuf.Length))
        {
            var framesString = Encoding.UTF8.GetString(_framesBuf);
            _frames = framesString[..framesString.IndexOf('\0')]
                .Split([' ', ',', ';'], StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.TryParse(x, out var n) ? n : -1)
                .Where(x => x >= 0)
                .ToArray();
        }

        int curFrame = _sprite.Frame;
        if (ImGui.SliderInt("Frame", ref curFrame, 0, _sprite.FrameCount - 1))
            _sprite.Frame = curFrame;

        ImGui.Checkbox("Animate", ref _isAnimating);
        ImGui.SliderFloat("Animation Speed", ref _animSpeed, 1.0f, 10.0f);

        TimeSpan period = TimeSpan.FromSeconds(1.0f / _animSpeed);
        if (_isAnimating && _lastTransition + period < DateTime.UtcNow && _frames.Length > 0)
        {
            _lastTransition = DateTime.UtcNow;
            _frameIndex++;

            int numInSeq = AnimUtil.GetFrame(_frameIndex, _frames.Length, false);
            int frame = _frames[numInSeq];
            _sprite.Frame = frame < _sprite.FrameCount ? frame : 0;
        }

        var fb = _fb.Color.DeviceTexture;
        if (fb == null)
            return;

        var imgui = Resolve<IImGuiManager>();
        var ptr1 = imgui.GetOrCreateImGuiBinding(fb);
        ImGui.Image(ptr1, new Vector2(fb.Width, fb.Height));
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
        cl.ClearColorTarget(0, RgbaFloat.Grey);
        cl.ClearDepthStencil(gd.IsDepthRangeZeroToOne ? 1f : 0f);

        var renderer = Resolve<IRenderManager>().GetRenderer(AlbionRenderSystemConstants.R_Sprite);
        _batchManager.Collect(_renderables);

        foreach (var renderable in _renderables)
            renderer.Render(renderable, _cl.CommandList, gd, _globalSet.ResourceSet, _passSet.ResourceSet);

        _cl.CommandList.End();

        gd.SubmitCommands(_cl.CommandList, _fence.Fence);
        gd.WaitForFence(_fence.Fence); // Slow but simple

        if (_fb.Width != (uint)_sprite.MaxSize.X * 2 && _sprite.MaxSize.X > 0)
            _fb.Width = (uint)_sprite.MaxSize.X * 2;

        if (_fb.Height != (uint)_sprite.MaxSize.Y * 2 && _sprite.MaxSize.Y > 0)
            _fb.Height = (uint)_sprite.MaxSize.Y * 2;
    }
}