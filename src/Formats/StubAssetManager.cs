﻿using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Api.Settings;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Flic;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats;

public class StubAssetManager : IAssetManager
{
    static readonly AlbionPalette Greyscale = new(0, "Greyscale", Enumerable.Range(0, 256).Select(x => ApiUtil.PackColor((byte)x, (byte)x, (byte)x, 255)).ToArray());
    public AlbionPalette LoadPalette(PaletteId id) => Greyscale;
    public AssetNode GetAssetInfo(AssetId id, string language = null) => throw new NotImplementedException();
    public Automap LoadAutomap(AutomapId id) => throw new NotImplementedException();
    public PartyMemberInfo LoadPartyMember(PartyMemberId id) => throw new NotImplementedException();
    public CharacterSheet LoadSheet(SheetId id) => throw new NotImplementedException();
    public EventSet LoadEventSet(EventSetId id) => throw new NotImplementedException();
    public FlicFile LoadVideo(VideoId id) => throw new NotImplementedException();
    public IList<Block> LoadBlockList(BlockListId id) => throw new NotImplementedException();
    public IList<IEvent> LoadScript(ScriptId id) => throw new NotImplementedException();
    public IMapData LoadMap(MapId id) => throw new NotImplementedException();
    public ISample LoadSample(SampleId id) => throw new NotImplementedException();
    public object LoadMapObject(MapObjectId id) => throw new NotImplementedException();
    public Ink LoadInk(InkId id) => throw new NotImplementedException();
    public FontDefinition LoadFontDefinition(FontId id) => throw new NotImplementedException();
    public MetaFont LoadFont(FontId fontId, InkId inkId) => throw new NotImplementedException();
    public ITexture LoadTexture(IAssetId id) => throw new NotImplementedException();
    public ITexture LoadTexture(SpriteId id) => throw new NotImplementedException();
    public ITileGraphics LoadTileGraphics(TilesetGfxId id) => throw new NotImplementedException();
    public Inventory LoadInventory(AssetId id) => throw new NotImplementedException();
    public ItemData LoadItem(ItemId id) => throw new NotImplementedException();
    public LabyrinthData LoadLabyrinthData(LabyrinthId id) => throw new NotImplementedException();
    public MonsterGroup LoadMonsterGroup(MonsterGroupId id) => throw new NotImplementedException();
    public SavedGame LoadSavedGame(string path) => throw new NotImplementedException();
    public SpellData LoadSpell(SpellId id) => throw new NotImplementedException();
    public TilesetData LoadTileData(TilesetId id) => throw new NotImplementedException();
    public WaveLib LoadWaveLib(WaveLibraryId waveLibraryId) => throw new NotImplementedException();
    public bool IsStringDefined(StringId id, string language) => throw new NotImplementedException();
    public bool IsStringDefined(TextId id, string language) => throw new NotImplementedException();
    public byte[] LoadSong(SongId id) => throw new NotImplementedException();
    public object LoadSoundBanks() => throw new NotImplementedException();
    public IVarSet LoadConfig() => throw new NotImplementedException();
    public InputConfig LoadInputConfig() => throw new NotImplementedException();
    public string LoadStringRaw(TextId id, string language = null) => throw new NotImplementedException();
    public string LoadStringRaw(StringId id, string language = null) => throw new NotImplementedException();
    public string LoadStringSafe(TextId id, string language) => throw new NotImplementedException();
    public string LoadStringSafe(StringId id, string language) => throw new NotImplementedException();
    public IStringSet LoadStringSet(StringSetId id, string language) => throw new NotImplementedException();
}
