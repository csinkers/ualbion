using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Reflection;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Sheets;
using UAlbion.Game.Events;

namespace UAlbion.Game.Veldrid.Diag;

public sealed class AssetViewerWindow : Component, IImGuiWindow
{
    AssetId _id;
    bool _dirty = true;
    object _asset;
    IAssetViewer _viewer;

    public AssetViewerWindow(string name)
    {
        Name = name;
        On<LanguageChangedEvent>(_ => _dirty = true);
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

        DrawInspector(Id.ToString(), _asset);
        _viewer?.Draw();

        ImGui.End();
        if (!open)
            Remove();
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

        var mods = Resolve<IModApplier>();
        _asset = mods.LoadAsset(_id);
        _viewer?.Remove();

        _viewer = AttachChild((IAssetViewer)(_asset switch
        {
            ITexture texture => new TextureViewer(texture),
            CharacterSheet _ => new CharacterViewer(),
            _ => null
        }));
    }
}