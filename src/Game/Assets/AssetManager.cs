﻿using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Settings;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Flic;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Assets;

public class AssetManager : Component, IAssetManager
{
    IModApplier _modApplier;

    public AssetManager() { }
    public AssetManager(IModApplier modApplier) => _modApplier = modApplier ?? throw new ArgumentNullException(nameof(modApplier));

    protected override void Subscribed()
    {
        _modApplier ??= Resolve<IModApplier>() ?? throw new InvalidOperationException("AssetManager is missing requirement of type IModApplier");
        Exchange.Register<IAssetManager>(this);
        Exchange.Register<ITextureLoader>(this);
    }

    protected override void Unsubscribed() => Exchange.Unregister(this);
    public AssetNode GetAssetInfo(AssetId id, string language = null) => _modApplier.GetAssetInfo(id, language);
    public IMapData LoadMap(MapId id) => (IMapData)_modApplier.LoadAsset(id); // No caching for map data
    public ItemData LoadItem(ItemId id) => (ItemData)_modApplier.LoadAssetCached(id);

    public AlbionPalette LoadPalette(PaletteId id)
    {
        var palette = (AlbionPalette)_modApplier.LoadAssetCached(id);
        if (palette == null)
            return null;

        var commonId = AssetId.From(Base.Palette.Common);
        if (palette.Id != commonId.ToUInt32())
        {
            var commonPalette = (AlbionPalette)_modApplier.LoadAssetCached(commonId);
            palette.SetCommonPalette(commonPalette);
        }

        return palette;
    }

    public ITexture LoadTexture(SpriteId id) => (ITexture)_modApplier.LoadAssetCached(id);
    public ITileGraphics LoadTileGraphics(TilesetGfxId id) => (ITileGraphics)_modApplier.LoadAssetCached(id);
    public object LoadMapObject(MapObjectId id) => _modApplier.LoadAssetCached(id);
    public ITexture LoadTexture(IAssetId id) => (ITexture)_modApplier.LoadAssetCached(SpriteId.FromUInt32(id?.ToUInt32() ?? 0));

    public Ink LoadInk(InkId id) => (Ink)_modApplier.LoadAssetCached(id);
    public FontDefinition LoadFontDefinition(FontId id) => (FontDefinition)_modApplier.LoadAssetCached(id);
    public MetaFont LoadFont(FontId fontId, InkId inkId) => (MetaFont)_modApplier.LoadAssetCached(new MetaFontId(fontId, inkId));

    TilesetData IAssetManager.LoadTileData(TilesetId id) => LoadTileData(id);
    LabyrinthData IAssetManager.LoadLabyrinthData(LabyrinthId id) => LoadLabyrinthData(id);
    public TilesetData LoadTileData(TilesetId id) => (TilesetData)_modApplier.LoadAssetCached(id);
    public LabyrinthData LoadLabyrinthData(LabyrinthId id) => (LabyrinthData)_modApplier.LoadAssetCached(id);

    public IStringSet LoadStringSet(StringSetId id) => LoadStringSet(id, null);
    public IStringSet LoadStringSet(StringSetId id, string language)
    {
        var currentLanguage = ReadVar(V.User.Gameplay.Language);
        bool cached = !(language != null && language != currentLanguage);
        language ??= currentLanguage;

        var asset = cached
            ? _modApplier.LoadAssetCached(id)
            : _modApplier.LoadAsset(id, language);

        return (IStringSet)asset;
    }

    string LoadStringCore(StringId id, string language, bool cached)
    {
        var currentLanguage = ReadVar(V.User.Gameplay.Language);
        if (language != null && language != currentLanguage)
            cached = false;
        language ??= currentLanguage;

        var asset = cached
            ? _modApplier.LoadAssetCached(id.Id)
            : _modApplier.LoadAsset(id.Id, language);

        return asset switch
        {
            IStringSet collection => collection.GetString(id),
            string s => s,
            _ => null
        };
    }
    public bool IsStringDefined(TextId id, string language) => LoadStringCore(new StringId(id), language, false) != null;
    public bool IsStringDefined(StringId id, string language) => LoadStringCore(id, language, false) != null;
    public string LoadStringRaw(TextId id, string language) => LoadStringCore(new StringId(id), language, true);
    public string LoadStringRaw(StringId id, string language) => LoadStringCore(id, language, true);
    public string LoadStringSafe(TextId id, string language) => LoadStringCore(new StringId(id), language, true) ?? $"!MISSING STRING {id.Id}!";
    public string LoadStringSafe(StringId id, string language) => LoadStringCore(id, language, true) ?? $"!MISSING STRING {id.Id}:{id.SubId}!";

    public ISample LoadSample(SampleId id) => (AlbionSample)_modApplier.LoadAssetCached(id);
    public WaveLib LoadWaveLib(WaveLibraryId waveLibraryId) => (WaveLib)_modApplier.LoadAssetCached(waveLibraryId);
    public FlicFile LoadVideo(VideoId id) => (FlicFile)_modApplier.LoadAssetCached(id);
    public PartyMemberInfo LoadPartyMember(PartyMemberId id) => (PartyMemberInfo)_modApplier.LoadAssetCached(id);
    public CharacterSheet LoadSheet(SheetId id) => (CharacterSheet)_modApplier.LoadAssetCached(id);
    public Inventory LoadInventory(AssetId id) => (Inventory)_modApplier.LoadAssetCached(id);
    public IList<Block> LoadBlockList(BlockListId id) => (IList<Block>)_modApplier.LoadAssetCached(id);
    public EventSet LoadEventSet(EventSetId id) => (EventSet)_modApplier.LoadAssetCached(id);
    public byte[] LoadSong(SongId id) => (byte[])_modApplier.LoadAssetCached(id);
    public IList<IEvent> LoadScript(ScriptId id) => (IList<IEvent>)_modApplier.LoadAsset(id);

    public SpellData LoadSpell(SpellId id) => (SpellData)_modApplier.LoadAssetCached(id);
    public SavedGame LoadSavedGame(string path) => _modApplier.LoadSavedGame(path);
    public MonsterGroup LoadMonsterGroup(MonsterGroupId id) => (MonsterGroup)_modApplier.LoadAssetCached(id);
    public Automap LoadAutomap(AutomapId id) => (Automap)_modApplier.LoadAssetCached(id);
    public object LoadSoundBanks() => _modApplier.LoadAssetCached(AssetId.From(Base.Special.SoundBank));
    public IVarSet LoadConfig() => (IVarSet)_modApplier.LoadAsset(AssetId.From(Base.Special.GameConfig));
    public InputConfig LoadInputConfig() => (InputConfig)_modApplier.LoadAsset(AssetId.From(Base.Special.InputConfig));
}
