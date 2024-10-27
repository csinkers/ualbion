using System;
using System.Linq;
using System.Numerics;
using System.Text;
using ImGuiNET;
using JetBrains.Annotations;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using VeldridGen.Interfaces;
using Component = UAlbion.Api.Eventing.Component;

namespace UAlbion.Game.Veldrid.Diag;

public sealed class TextureViewer : Component, IAssetViewer
{
    readonly byte[] _framesBuf = new byte[256];
    readonly ITexture _asset;
    readonly TextureViewerRenderer _renderer;

    int[] _frames = [];
    int _frameIndex;
    DateTime _lastTransition = DateTime.UtcNow;
    float _animSpeed = 1.0f;
    bool _isAnimating;

    string[] _paletteNames = [];
    AssetId[] _paletteIds = [];
    ITextureArrayHolder _textureArray;
    ITextureHolder _texture;
    int _curPal;
    int _defaultPalette;
    bool _skipShadows;
    bool _isBouncy;

    public TextureViewer([NotNull] ITexture asset)
    {
        if (asset is SimpleTexture<byte> tex) // Add white border to all regions.
        {
            var clone = tex.Clone();
            foreach (var region in clone.Regions)
            {
                var buf = clone.GetMutableRegionBuffer(region);
                buf.GetRow(0).Fill(255);
                buf.GetRow(buf.Height - 1).Fill(255);
                for (int i = 1; i < buf.Height - 1; i++)
                {
                    var row = buf.GetRow(i);
                    row[0] = 255;
                    row[buf.Width - 1] = 255;
                }
            }

            asset = clone;
        }

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

        _paletteIds = AssetMapping.Global.EnumerateAssetsOfType(AssetType.Palette).OrderBy(x => x.ToString()).ToArray();
        _paletteNames = _paletteIds.Select(x => x.ToString()).ToArray();

        var meta = Resolve<IAssetManager>().GetAssetInfo((AssetId)_asset.Id);
        var palId = meta.PaletteId;
        if (palId.IsNone)
            palId = _paletteIds[0];

        _skipShadows = ((AssetId)_asset.Id).Type == AssetType.MonsterGfx;

        for (int i = 0; i < _paletteIds.Length; i++)
            if (palId == _paletteIds[i])
                _curPal = i;

        _defaultPalette = _curPal;
        AlbionPalette pal = Resolve<IAssetManager>().LoadPalette(palId);
        var textureSource = Resolve<ITextureSource>();
        _renderer.Palette = textureSource.GetSimpleTexture(pal.Texture);
    }

    public void Draw()
    {
        int zoom = _renderer.Zoom;
        if (ImGui.SliderInt("Zoom", ref zoom, 0, 4))
            _renderer.Zoom = zoom;

        bool paletteChanged = ImGui.Combo("Palette", ref _curPal, _paletteNames, _paletteNames.Length);
        ImGui.SameLine();
        if (ImGui.Button("+##pal"))
        {
            paletteChanged = true;
            _curPal++;
            if (_curPal >= _paletteIds.Length)
                _curPal = 0;
        }

        ImGui.SameLine();
        if (ImGui.Button("-##pal"))
        {
            paletteChanged = true;
            _curPal--;
            if (_curPal < 0)
                _curPal = _paletteIds.Length - 1;
        }

        ImGui.SameLine();
        if (ImGui.Button("def##pal"))
        {
            paletteChanged = true;
            _curPal = _defaultPalette;
        }

        if (paletteChanged)
        {
            AlbionPalette pal = Resolve<IAssetManager>().LoadPalette(_paletteIds[_curPal]);
            var textureSource = Resolve<ITextureSource>();
            _renderer.Palette = textureSource.GetSimpleTexture(pal.Texture);
        }

        int frameSkipFactor = _skipShadows ? 2 : 1; 
        ImGui.Text($"Max Frame: {_renderer.FrameCount / frameSkipFactor}");

        bool temp = ImGui.Checkbox("Skip shadows", ref _skipShadows);
        if (ImGui.InputText("Frames", _framesBuf, (uint)_framesBuf.Length) || temp)
        {
            var framesString = Encoding.UTF8.GetString(_framesBuf);
            _frames = framesString[..framesString.IndexOf('\0')]
                .Split([' ', ',', ';'], StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.TryParse(x, out var n) ? n : -1)
                .Where(x => x >= 0)
                .ToArray();
        }

        int curFrame = _renderer.Frame / frameSkipFactor;
        if (ImGui.SliderInt("Frame", ref curFrame, 0, (_renderer.FrameCount / frameSkipFactor) - 1))
            _renderer.Frame = curFrame * frameSkipFactor;

        ImGui.Checkbox("Animate", ref _isAnimating);
        ImGui.Checkbox("Bouncy", ref _isBouncy);
        ImGui.SliderFloat("Animation Speed", ref _animSpeed, 1.0f, 10.0f);

        TimeSpan period = TimeSpan.FromSeconds(1.0f / _animSpeed);
        if (_isAnimating && _lastTransition + period < DateTime.UtcNow && _frames.Length > 0)
        {
            _lastTransition = DateTime.UtcNow;
            _frameIndex++;

            int numInSeq = AnimUtil.GetFrame(_frameIndex, _frames.Length, _isBouncy);
            int frame = _frames[numInSeq] * frameSkipFactor;
            _renderer.Frame = frame < _renderer.FrameCount ? frame : 0;
        }

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