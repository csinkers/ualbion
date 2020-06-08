using System;

namespace UAlbion.Formats.AssetIds
{
    public struct AssetId : IConvertible, IEquatable<AssetId>
    {
        public AssetId(AssetType type, ushort id)
        {
            Type = type;
            Id = id;
        }

        public AssetType Type { get; }
        public ushort Id { get; }

        public override string ToString() => Type + ":" + (Type switch
        {
            AssetType.Automap            => ((AutoMapId)Id).ToString(),
            AssetType.AutomapGraphics    => ((AutoMapId)Id).ToString(),
            AssetType.BlockList          => ((BlockListId)Id).ToString(),
            AssetType.ChestData          => ((ChestId)Id).ToString(),
            AssetType.CombatBackground   => ((CombatBackgroundId)Id).ToString(),
            AssetType.CombatGraphics     => ((CombatGraphicsId)Id).ToString(),
            AssetType.CoreGraphics       => ((CoreSpriteId)Id).ToString(),
            AssetType.BackgroundGraphics => ((DungeonBackgroundId)Id).ToString(),
            AssetType.Floor3D            => ((DungeonFloorId)Id).ToString(),
            AssetType.Object3D           => ((DungeonObjectId)Id).ToString(),
            AssetType.Overlay3D          => ((DungeonOverlayId)Id).ToString(),
            AssetType.Wall3D             => ((DungeonWallId)Id).ToString(),
            AssetType.EventSet           => ((EventSetId)Id).ToString(),
            AssetType.EventText          => ((EventTextId)Id).ToString(),
            AssetType.Font               => ((FontId)Id).ToString(),
            AssetType.FullBodyPicture    => ((FullBodyPictureId)Id).ToString(),
            AssetType.Tileset            => ((TilesetId)Id).ToString(),
            AssetType.IconGraphics       => ((IconGraphicsId)Id).ToString(),
            AssetType.ItemGraphics       => ((ItemSpriteId)Id).ToString(),
            AssetType.ItemList           => ((ItemId)Id).ToString(),
            AssetType.ItemNames          => ((ItemId)Id).ToString(),
            AssetType.LabData            => ((LabyrinthDataId)Id).ToString(),
            AssetType.BigNpcGraphics     => ((LargeNpcId)Id).ToString(),
            AssetType.BigPartyGraphics   => ((LargePartyGraphicsId)Id).ToString(),
            AssetType.MapData            => ((MapDataId)Id).ToString(),
            AssetType.MapText            => ((MapTextId)Id).ToString(),
            AssetType.MerchantData       => ((MerchantId)Id).ToString(),
            AssetType.Monster            => ((MonsterCharacterId)Id).ToString(),
            AssetType.MonsterGraphics    => ((MonsterGraphicsId)Id).ToString(),
            AssetType.MonsterGroup       => ((MonsterGroupId)Id).ToString(),
            AssetType.Npc                => ((NpcCharacterId)Id).ToString(),
            AssetType.Palette            => ((PaletteId)Id).ToString(),
            AssetType.PartyMember        => ((PartyCharacterId)Id).ToString(),
            AssetType.Picture            => ((PictureId)Id).ToString(),
            AssetType.Sample             => ((SampleId)Id).ToString(),
            AssetType.Script             => ((ScriptId)Id).ToString(),
            AssetType.SmallNpcGraphics   => ((SmallNpcId)Id).ToString(),
            AssetType.SmallPartyGraphics => ((SmallPartyGraphicsId)Id).ToString(),
            AssetType.SmallPortrait      => ((SmallPortraitId)Id).ToString(),
            AssetType.Song               => ((SongId)Id).ToString(),
            AssetType.SpellData          => ((SpellId)Id).ToString(),
            AssetType.SystemText         => ((SystemTextId)Id).ToString(),
            AssetType.TacticalIcon       => ((TacticId)Id).ToString(),
            AssetType.TransparencyTables => ((TranslationTableId)Id).ToString(),
            AssetType.Flic               => ((VideoId)Id).ToString(),
            AssetType.WaveLibrary        => ((WaveLibraryId)Id).ToString(),
            AssetType.Dictionary         => ((WordId)Id).ToString(),
            AssetType.UAlbionText        => ((UAlbionStringId)Id).ToString(),
            _ => Id.ToString()
        });

        public static explicit operator int(AssetId id) => (int)id.Type << 16 | id.Id;
        public static explicit operator AssetId(int id)
            => new AssetId(
                (AssetType)((id & 0x7fff0000) >> 16),
                (ushort)(id & 0xffff));

        public static implicit operator AssetId(AutoMapId id)            => new AssetId(AssetType.Automap,            (ushort)id);
        public static implicit operator AssetId(BlockListId id)          => new AssetId(AssetType.BlockList,          (ushort)id);
        public static implicit operator AssetId(ChestId id)              => new AssetId(AssetType.ChestData,          (ushort)id);
        public static implicit operator AssetId(CombatBackgroundId id)   => new AssetId(AssetType.CombatBackground,   (ushort)id);
        public static implicit operator AssetId(CombatGraphicsId id)     => new AssetId(AssetType.CombatGraphics,     (ushort)id);
        public static implicit operator AssetId(CoreSpriteId id)         => new AssetId(AssetType.CoreGraphics,       (ushort)id);
        public static implicit operator AssetId(DungeonBackgroundId id)  => new AssetId(AssetType.BackgroundGraphics, (ushort)id);
        public static implicit operator AssetId(DungeonFloorId id)       => new AssetId(AssetType.Floor3D,            (ushort)id);
        public static implicit operator AssetId(DungeonObjectId id)      => new AssetId(AssetType.Object3D,           (ushort)id);
        public static implicit operator AssetId(DungeonOverlayId id)     => new AssetId(AssetType.Overlay3D,          (ushort)id);
        public static implicit operator AssetId(DungeonWallId id)        => new AssetId(AssetType.Wall3D,             (ushort)id);
        public static implicit operator AssetId(EventSetId id)           => new AssetId(AssetType.EventSet,           (ushort)id);
        public static implicit operator AssetId(EventTextId id)          => new AssetId(AssetType.EventText,          (ushort)id);
        public static implicit operator AssetId(FontId id)               => new AssetId(AssetType.Font,               (ushort)id);
        public static implicit operator AssetId(FullBodyPictureId id)    => new AssetId(AssetType.FullBodyPicture,    (ushort)id);
        public static implicit operator AssetId(TilesetId id)            => new AssetId(AssetType.Tileset,            (ushort)id);
        public static implicit operator AssetId(IconGraphicsId id)       => new AssetId(AssetType.IconGraphics,       (ushort)id);
        public static implicit operator AssetId(ItemSpriteId id)         => new AssetId(AssetType.ItemGraphics,       (ushort)id);
        public static implicit operator AssetId(ItemId id)               => new AssetId(AssetType.ItemList,           (ushort)id);
        public static implicit operator AssetId(LabyrinthDataId id)      => new AssetId(AssetType.LabData,            (ushort)id);
        public static implicit operator AssetId(LargeNpcId id)           => new AssetId(AssetType.BigNpcGraphics,     (ushort)id);
        public static implicit operator AssetId(LargePartyGraphicsId id) => new AssetId(AssetType.BigPartyGraphics,   (ushort)id);
        public static implicit operator AssetId(MapDataId id)            => new AssetId(AssetType.MapData,            (ushort)id);
        public static implicit operator AssetId(MapTextId id)            => new AssetId(AssetType.MapText,            (ushort)id);
        public static implicit operator AssetId(MerchantId id)           => new AssetId(AssetType.MerchantData,       (ushort)id);
        public static implicit operator AssetId(MonsterCharacterId id)   => new AssetId(AssetType.Monster,            (ushort)id);
        public static implicit operator AssetId(MonsterGraphicsId id)    => new AssetId(AssetType.MonsterGraphics,    (ushort)id);
        public static implicit operator AssetId(MonsterGroupId id)       => new AssetId(AssetType.MonsterGroup,       (ushort)id);
        public static implicit operator AssetId(NpcCharacterId id)       => new AssetId(AssetType.Npc,                (ushort)id);
        public static implicit operator AssetId(PaletteId id)            => new AssetId(AssetType.Palette,            (ushort)id);
        public static implicit operator AssetId(PartyCharacterId id)     => new AssetId(AssetType.PartyMember,        (ushort)id);
        public static implicit operator AssetId(PictureId id)            => new AssetId(AssetType.Picture,            (ushort)id);
        public static implicit operator AssetId(SampleId id)             => new AssetId(AssetType.Sample,             (ushort)id);
        public static implicit operator AssetId(ScriptId id)             => new AssetId(AssetType.Script,             (ushort)id);
        public static implicit operator AssetId(SmallNpcId id)           => new AssetId(AssetType.SmallNpcGraphics,   (ushort)id);
        public static implicit operator AssetId(SmallPartyGraphicsId id) => new AssetId(AssetType.SmallPartyGraphics, (ushort)id);
        public static implicit operator AssetId(SmallPortraitId id)      => new AssetId(AssetType.SmallPortrait,      (ushort)id);
        public static implicit operator AssetId(SongId id)               => new AssetId(AssetType.Song,               (ushort)id);
        public static implicit operator AssetId(SpellId id)              => new AssetId(AssetType.SpellData,          (ushort)id);
        public static implicit operator AssetId(SystemTextId id)         => new AssetId(AssetType.SystemText,         (ushort)id);
        public static implicit operator AssetId(TacticId id)             => new AssetId(AssetType.TacticalIcon,       (ushort)id);
        public static implicit operator AssetId(TranslationTableId id)   => new AssetId(AssetType.TransparencyTables, (ushort)id);
        public static implicit operator AssetId(VideoId id)              => new AssetId(AssetType.Flic,               (ushort)id);
        public static implicit operator AssetId(WaveLibraryId id)        => new AssetId(AssetType.WaveLibrary,        (ushort)id);
        public static implicit operator AssetId(WordId id)               => new AssetId(AssetType.Dictionary,         (ushort)id);
        public static implicit operator AssetId(UAlbionStringId id)      => new AssetId(AssetType.UAlbionText,        (ushort)id);

        public int ToInt32(IFormatProvider provider) => (int)this;
        public TypeCode GetTypeCode() => throw new NotImplementedException();
        public bool ToBoolean(IFormatProvider provider) => throw new NotImplementedException();
        public byte ToByte(IFormatProvider provider) => throw new NotImplementedException();
        public char ToChar(IFormatProvider provider) => throw new NotImplementedException();
        public DateTime ToDateTime(IFormatProvider provider) => throw new NotImplementedException();
        public decimal ToDecimal(IFormatProvider provider) => throw new NotImplementedException();
        public double ToDouble(IFormatProvider provider) => throw new NotImplementedException();
        public short ToInt16(IFormatProvider provider) => throw new NotImplementedException();
        public long ToInt64(IFormatProvider provider) => throw new NotImplementedException();
        public sbyte ToSByte(IFormatProvider provider) => throw new NotImplementedException();
        public float ToSingle(IFormatProvider provider) => throw new NotImplementedException();
        public string ToString(IFormatProvider provider) => throw new NotImplementedException();
        public ushort ToUInt16(IFormatProvider provider) => throw new NotImplementedException();
        public uint ToUInt32(IFormatProvider provider) => throw new NotImplementedException();
        public ulong ToUInt64(IFormatProvider provider) => throw new NotImplementedException();
        public object ToType(Type conversionType, IFormatProvider provider) => throw new NotImplementedException();

        public static bool operator ==(AssetId x, AssetId y) => x.Equals(y);
        public static bool operator !=(AssetId x, AssetId y) => !(x == y);
        public bool Equals(AssetId other) => Type == other.Type && Id == other.Id;
        public override bool Equals(object obj) => obj is AssetId other && Equals(other);
        public override int GetHashCode() => (int)this;
    }
}
