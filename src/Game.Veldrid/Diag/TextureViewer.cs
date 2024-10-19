using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using JetBrains.Annotations;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using Veldrid;
using VeldridGen.Interfaces;
using Component = UAlbion.Api.Eventing.Component;

namespace UAlbion.Game.Veldrid.Diag;

public class TextureViewerRenderer : Component, ICameraProvider
{
    public uint Width
    {
        get => _fb.Width;
        set => _fb.Width = value;
    }

    public uint Height
    {
        get => _fb.Height;
        set => _fb.Height = value;
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

    public TextureViewerRenderer(ITexture texture)
    {
        _fb = AttachChild(new SimpleFramebuffer("fb_texViewer", (uint)texture.Width, (uint)texture.Height));
        _cl = AttachChild(new CommandListHolder("cl_texViewer"));
        _fence = AttachChild(new FenceHolder("f_texViewer"));
        _batchManager = AttachChild(new BatchManager<SpriteKey, SpriteInfo>(static (key, f) => f.CreateSpriteBatch(key), false));
        _globalSet = AttachChild(new GlobalResourceSetProvider("TexViewerGlobal"));
        _camera = AttachChild(new OrthographicCamera());
        _passSet = AttachChild(new MainPassResourceProvider(_fb, this));
        _sprite = AttachChild(new Sprite(AssetId.None, DrawLayer.Interface, SpriteKeyFlags.NoTransform, 0, _ => texture, _batchManager));

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

public sealed class TextureViewer : Component, IAssetViewer
{
    readonly byte[] _framesBuf = new byte[256];
    readonly ITexture _asset;
    readonly TextureViewerRenderer _renderer;

    string[] _paletteNames = [];
    ITextureArrayHolder _textureArray;
    ITextureHolder _texture;
    int _curPal;
    float _animSpeed;

    public TextureViewer([NotNull] ITexture asset)
    {
        _asset = asset ?? throw new ArgumentNullException(nameof(asset));
        _renderer = AttachChild(new TextureViewerRenderer(asset));
    }

    protected override void Subscribed()
    {
        var source = Resolve<ITextureSource>();
        if (_asset.ArrayLayers > 1)
            _textureArray = source.GetArrayTexture(_asset);
        else
            _texture = source.GetSimpleTexture(_asset);

        var paletteIds = AssetMapping.Global.EnumerateAssetsOfType(AssetType.Palette);
        _paletteNames = paletteIds.Select(x => x.ToString()).OrderBy(x => x).ToArray();

        AlbionPalette pal = Resolve<IAssetManager>().LoadPalette(paletteIds.First());
        var textureSource = Resolve<ITextureSource>();
        _renderer.Palette = textureSource.GetSimpleTexture(pal.Texture);
    }

    public void Draw()
    {
        ImGui.Combo("Palette", ref _curPal, _paletteNames, _paletteNames.Length);
        ImGui.Text($"Max Frame: {_renderer.FrameCount}");

        if (ImGui.InputText("Frames", _framesBuf, (uint)_framesBuf.Length))
        {
        }

        ImGui.SliderFloat("Animation Speed", ref _animSpeed, 0.0f, 1.0f);

        if (_texture is { DeviceTexture: not null })
        {
            var imgui = Resolve<IImGuiManager>();

            var ptr1 = imgui.GetOrCreateImGuiBinding(_renderer.Framebuffer);
            ImGui.Image(ptr1, new Vector2(_renderer.Width, _renderer.Height));

            ImGui.Text("Raw:");
            var ptr2 = imgui.GetOrCreateImGuiBinding(_texture.DeviceTexture);
            ImGui.Image(ptr2, new Vector2(_texture.DeviceTexture.Width, _texture.DeviceTexture.Height));
        }

        if (_textureArray is { DeviceTexture: not null })
        {
            ImGui.Text("TextureArray: TODO");
        }
    }
}