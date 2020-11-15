// Note: This file was automatically generated using Tools/GenerateEnums.
// No changes should be made to this file by hand. Instead, the relevant json
// files should be modified and then GenerateEnums should be used to regenerate
// the various types.
using System;
using System.ComponentModel;
using System.Globalization;
using Newtonsoft.Json;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Assets
{
    [JsonConverter(typeof(ToStringJsonConverter))]
    [TypeConverter(typeof(PartyMemberIdConverter))]
    public struct PartyMemberId : IEquatable<PartyMemberId>, IEquatable<AssetId>, ITextureId
    {
        readonly uint _value;
        public PartyMemberId(AssetType type, int id = 0)
        {
            if (!(type == AssetType.None || type == AssetType.PartyMember))
                throw new ArgumentOutOfRangeException($"Tried to construct a PartyMemberId with a type of {type}");
#if DEBUG
            if (id < 0 || id > 0xffffff)
                throw new ArgumentOutOfRangeException($"Tried to construct a PartyMemberId with out of range id {id}");
#endif
            _value = (uint)type << 24 | (uint)id;
        }

        public PartyMemberId(uint id) 
        { 
            _value = id;
            if (!(Type == AssetType.None || Type == AssetType.PartyMember))
                throw new ArgumentOutOfRangeException($"Tried to construct a PartyMemberId with a type of {Type}");
        }
        public PartyMemberId(int id)
        {
            _value = unchecked((uint)id);
            if (!(Type == AssetType.None || Type == AssetType.PartyMember))
                throw new ArgumentOutOfRangeException($"Tried to construct a PartyMemberId with a type of {Type}");
        }

        public static PartyMemberId From<T>(T id) where T : unmanaged, Enum => (PartyMemberId)AssetMapping.Global.EnumToId(id);

        public int ToDisk(AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
            return mapping.EnumToId(enumType, enumValue).Id;
        }

        public static PartyMemberId FromDisk(int disk, AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            

            var (enumType, enumValue) = mapping.IdToEnum(new PartyMemberId(AssetType.PartyMember, disk));
            return (PartyMemberId)AssetMapping.Global.EnumToId(enumType, enumValue);
        }

        public static PartyMemberId SerdesU8(string name, PartyMemberId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            byte diskValue = (byte)id.ToDisk(mapping);
            diskValue = s.UInt8(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public static PartyMemberId SerdesU16(string name, PartyMemberId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
        public readonly int Id => (int)(_value & 0xffffff);
        public static PartyMemberId None => new PartyMemberId(AssetType.None);
        public bool IsNone => Type == AssetType.None;

        public override string ToString() => AssetMapping.Global.IdToName(this);
        static AssetType[] _validTypes = { AssetType.PartyMember };
        public static PartyMemberId Parse(string s) => AssetMapping.Global.Parse(s, _validTypes);

        public static implicit operator AssetId(PartyMemberId id) => new AssetId(id._value);
        public static implicit operator PartyMemberId(AssetId id) => new PartyMemberId((uint)id);
        public static explicit operator uint(PartyMemberId id) => id._value;
        public static explicit operator int(PartyMemberId id) => unchecked((int)id._value);
        public static explicit operator PartyMemberId(int id) => new PartyMemberId(id);
        public static implicit operator CharacterId(PartyMemberId id) => new CharacterId(id._value);
        public static explicit operator PartyMemberId(CharacterId id) => new PartyMemberId((uint)id);
        public static implicit operator PartyMemberId(UAlbion.Base.PartyMember id) => PartyMemberId.From(id);

        public static PartyMemberId ToPartyMemberId(int id) => new PartyMemberId(id);
        public readonly int ToInt32() => (int)this;
        public readonly uint ToUInt32() => (uint)this;
        public static bool operator ==(PartyMemberId x, PartyMemberId y) => x.Equals(y);
        public static bool operator !=(PartyMemberId x, PartyMemberId y) => !(x == y);
        public static bool operator ==(PartyMemberId x, AssetId y) => x.Equals(y);
        public static bool operator !=(PartyMemberId x, AssetId y) => !(x == y);
        public bool Equals(PartyMemberId other) => _value == other._value;
        public bool Equals(AssetId other) => _value == (uint)other;
        public override bool Equals(object obj) => obj is ITextureId other && Equals(other);
        public override int GetHashCode() => (int)this;
        public readonly SpriteId ToSmallPartyGraphics() => new SpriteId(AssetType.SmallPartyGraphics, Id);
        public readonly SpriteId ToBigPartyGraphics() => new SpriteId(AssetType.BigPartyGraphics, Id);
        public readonly SpriteId ToFullBodyPicture() => new SpriteId(AssetType.FullBodyPicture, Id);
    }

    public class PartyMemberIdConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
            => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
            => value is string s ? PartyMemberId.Parse(s) : base.ConvertFrom(context, culture, value);

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) =>
            destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
    }
}