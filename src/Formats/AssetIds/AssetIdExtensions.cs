using System;

namespace UAlbion.Formats.AssetIds
{
    public static class AssetIdExtensions
    {
        public static SmallPortraitId ToPortraitId(this PartyCharacterId partyCharacterId) =>
            partyCharacterId switch
            {
                PartyCharacterId.Tom      => SmallPortraitId.Tom,
                PartyCharacterId.Rainer   => SmallPortraitId.Rainer,
                PartyCharacterId.Drirr    => SmallPortraitId.Drirr,
                PartyCharacterId.Sira     => SmallPortraitId.Sira,
                PartyCharacterId.Mellthas => SmallPortraitId.Mellthas,
                PartyCharacterId.Harriet  => SmallPortraitId.Harriet,
                PartyCharacterId.Joe      => SmallPortraitId.Joe,
                PartyCharacterId.Unknown7 => SmallPortraitId.Unknown7,
                PartyCharacterId.Khunag   => SmallPortraitId.Khunag,
                PartyCharacterId.Siobhan  => SmallPortraitId.Siobhan,
                _ => throw new ArgumentOutOfRangeException(nameof(partyCharacterId), partyCharacterId, null)
            };

        public static AssetId ToAssetId(this AutoMapId id) => new AssetId(AssetType.Automap, (ushort)id);
        public static AssetId ToAssetId(this BlockListId id) => new AssetId(AssetType.BlockList, (ushort)id);
        public static AssetId ToAssetId(this ChestId id) => new AssetId(AssetType.ChestData, (ushort)id);
        public static AssetId ToAssetId(this CombatBackgroundId id) => new AssetId(AssetType.CombatBackground, (ushort)id);
        public static AssetId ToAssetId(this CombatGraphicsId id) => new AssetId(AssetType.CombatGraphics, (ushort)id);
        public static AssetId ToAssetId(this CoreSpriteId id) => new AssetId(AssetType.CoreGraphics, (ushort)id);
        public static AssetId ToAssetId(this DungeonBackgroundId id) => new AssetId(AssetType.BackgroundGraphics, (ushort)id);
        public static AssetId ToAssetId(this DungeonFloorId id) => new AssetId(AssetType.Floor3D, (ushort)id);
        public static AssetId ToAssetId(this DungeonObjectId id) => new AssetId(AssetType.Object3D, (ushort)id);
        public static AssetId ToAssetId(this DungeonOverlayId id) => new AssetId(AssetType.Overlay3D, (ushort)id);
        public static AssetId ToAssetId(this DungeonWallId id) => new AssetId(AssetType.Wall3D, (ushort)id);
        public static AssetId ToAssetId(this EventSetId id) => new AssetId(AssetType.EventSet, (ushort)id);
        public static AssetId ToAssetId(this EventTextId id) => new AssetId(AssetType.EventText, (ushort)id);
        public static AssetId ToAssetId(this FontId id) => new AssetId(AssetType.Font, (ushort)id);
        public static AssetId ToAssetId(this FullBodyPictureId id) => new AssetId(AssetType.FullBodyPicture, (ushort)id);
        public static AssetId ToAssetId(this TilesetId id) => new AssetId(AssetType.Tileset, (ushort)id);
        public static AssetId ToAssetId(this IconGraphicsId id) => new AssetId(AssetType.IconGraphics, (ushort)id);
        public static AssetId ToAssetId(this ItemSpriteId id) => new AssetId(AssetType.ItemGraphics, (ushort)id);
        public static AssetId ToAssetId(this ItemId id) => new AssetId(AssetType.ItemList, (ushort)id);
        public static AssetId ToAssetId(this LabyrinthDataId id) => new AssetId(AssetType.LabData, (ushort)id);
        public static AssetId ToAssetId(this LargeNpcId id) => new AssetId(AssetType.BigNpcGraphics, (ushort)id);
        public static AssetId ToAssetId(this LargePartyGraphicsId id) => new AssetId(AssetType.BigPartyGraphics, (ushort)id);
        public static AssetId ToAssetId(this MapDataId id) => new AssetId(AssetType.MapData, (ushort)id);
        public static AssetId ToAssetId(this MapTextId id) => new AssetId(AssetType.MapText, (ushort)id);
        public static AssetId ToAssetId(this MerchantId id) => new AssetId(AssetType.MerchantData, (ushort)id);
        public static AssetId ToAssetId(this MonsterCharacterId id) => new AssetId(AssetType.Monster, (ushort)id);
        public static AssetId ToAssetId(this MonsterGraphicsId id) => new AssetId(AssetType.MonsterGraphics, (ushort)id);
        public static AssetId ToAssetId(this MonsterGroupId id) => new AssetId(AssetType.MonsterGroup, (ushort)id);
        public static AssetId ToAssetId(this NpcCharacterId id) => new AssetId(AssetType.Npc, (ushort)id);
        public static AssetId ToAssetId(this PaletteId id) => new AssetId(AssetType.Palette, (ushort)id);
        public static AssetId ToAssetId(this PartyCharacterId id) => new AssetId(AssetType.PartyMember, (ushort)id);
        public static AssetId ToAssetId(this PictureId id) => new AssetId(AssetType.Picture, (ushort)id);
        public static AssetId ToAssetId(this SampleId id) => new AssetId(AssetType.Sample, (ushort)id);
        public static AssetId ToAssetId(this ScriptId id) => new AssetId(AssetType.Script, (ushort)id);
        public static AssetId ToAssetId(this SmallNpcId id) => new AssetId(AssetType.SmallNpcGraphics, (ushort)id);
        public static AssetId ToAssetId(this SmallPartyGraphicsId id) => new AssetId(AssetType.SmallPartyGraphics, (ushort)id);
        public static AssetId ToAssetId(this SmallPortraitId id) => new AssetId(AssetType.SmallPortrait, (ushort)id);
        public static AssetId ToAssetId(this SongId id) => new AssetId(AssetType.Song, (ushort)id);
        public static AssetId ToAssetId(this SpellId id) => new AssetId(AssetType.SpellData, (ushort)id);
        public static AssetId ToAssetId(this SystemTextId id) => new AssetId(AssetType.SystemText, (ushort)id);
        public static AssetId ToAssetId(this TacticId id) => new AssetId(AssetType.TacticalIcon, (ushort)id);
        public static AssetId ToAssetId(this TranslationTableId id) => new AssetId(AssetType.TransparencyTables, (ushort)id);
        public static AssetId ToAssetId(this VideoId id) => new AssetId(AssetType.Flic, (ushort)id);
        public static AssetId ToAssetId(this WaveLibraryId id) => new AssetId(AssetType.WaveLibrary, (ushort)id);
        public static AssetId ToAssetId(this WordId id) => new AssetId(AssetType.Dictionary, (ushort)id);
        public static AssetId ToAssetId(this UAlbionStringId id) => new AssetId(AssetType.UAlbionText, (ushort)id);
    }
}
