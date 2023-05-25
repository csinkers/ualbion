using System.Numerics;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Reflection;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Formats;
using VeldridGen.Interfaces;

namespace UAlbion.Game.Veldrid.Diag;

public sealed class AssetViewerWindow : Component, IImGuiWindow
{
    AssetId _id;
    bool _dirty = true;
    object _asset;
    ITextureArrayHolder _textureArray;
    ITextureHolder _texture;

    public AssetViewerWindow(string name)
    {
        Name = name;
    }

    public AssetId Id
    {
        get => _id;
        set
        {
            if (_id == value) return;
            _id = value;
            _dirty = true;
        }
    }

    public string Name { get; }
    public void Draw()
    {
        Refresh();

        bool open = true;
        ImGui.Begin(Name, ref open);

        DrawTexture(_asset as ITexture);
        DrawInspector(Id.ToString(), _asset);

        ImGui.End();
        if (!open)
            Remove();
    }

    void DrawTexture(ITexture texture)
    {
        if (_texture != null)
        {
            var imgui = Resolve<IImGuiManager>();
            var ptr = imgui.GetOrCreateImGuiBinding(_texture.DeviceTexture);
            ImGui.Image(ptr, new Vector2(_texture.DeviceTexture.Width, _texture.DeviceTexture.Height));
        }

        if (_textureArray != null)
        {
        }
    }

    static void DrawInspector(string name, object target)
    {
        var meta = new ReflectorMetadata(name, null, null, null);
        var state = new ReflectorState(target, null, -1, meta);
        var reflector = ReflectorManager.Instance.GetReflectorForInstance(state.Target);
        reflector(state);
    }

    void Refresh()
    {
        if (!_dirty)
            return;

        _dirty = false;
        _textureArray = null;
        _texture = null;

        var mods = Resolve<IModApplier>();
        _asset = mods.LoadAsset(_id);

        if (_asset is ITexture texture)
        {
            var source = Resolve<ITextureSource>();
            if (texture.ArrayLayers > 1)
            {
                _textureArray = source.GetArrayTexture(texture);
            }
            else
                _texture = source.GetSimpleTexture(texture);
        }
    }
}