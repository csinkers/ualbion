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

        public override string ToString() => Type switch
        {
            AssetType.AssetConfig          => "AssetConfig",
            AssetType.CoreSpriteConfig     => "CoreSpriteConfig",
            AssetType.CoreGraphicsMetadata => "CoreGraphicsMetadata",
            AssetType.GeneralConfig        => "GeneralConfig",
            AssetType.MetaFont             => "MetaFont:" + Id,
            AssetType.PaletteNull          => "PaletteNull",
            AssetType.SoundBank            => "SoundBank",
            AssetType.Slab                 => "SLAB",
            AssetType.Unnamed2             => "Unnamed2",
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
        };

        public static explicit operator int(AssetId id) => (int)id.Type << 16 | id.Id;
        public static explicit operator AssetId(int id) 
            => new AssetId(
                (AssetType)((id & 0x7fff0000) >> 16),
                (ushort)(id & 0xffff));

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

        public bool Equals(AssetId other) => Type == other.Type && Id == other.Id;
        public override bool Equals(object obj) => obj is AssetId other && Equals(other);
        public override int GetHashCode() => (int)this;
    }
}
