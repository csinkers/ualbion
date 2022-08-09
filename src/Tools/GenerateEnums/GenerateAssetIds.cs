using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UAlbion.Config;

namespace UAlbion.CodeGenerator;

static class GenerateAssetIds
{
    public static void Generate(Assets assets, string destinationPath, string destinationNamespace)
    {
        foreach (var assetType in assets.AssetIdConfig.Mappings)
        {
            var outputPath = Path.Combine(assets.BaseDir, destinationPath, assetType.Key + ".g.cs");
            var text = BuildClass(assets, destinationNamespace, assetType.Key, assetType.Value);
            File.WriteAllText(outputPath, text);
        }
    }

    static string BuildIfCondition(IList<AssetType> types, string comparandName)
    {
        var runs = 
            types
                .Union(new[] { AssetType.None })
                .Select(x => (int)x)
                .OrderBy(x => x)
                .Aggregate(new List<(int, int)>(), (acc, x) =>
                {
                    if (acc.Count == 0) acc.Add((x, x));
                    else if (acc[^1].Item2 == x - 1) acc[^1] = (acc[^1].Item1, x);
                    else acc.Add((x, x));
                    return acc;
                });

        var sb = new StringBuilder();
        bool first = true;
        foreach (var (start, end) in runs)
        {
            if (!first)
                sb.Append(" || ");
            first = false;

            if (start == end)
            {
                sb.Append(comparandName);
                sb.Append(" == AssetType.");
                sb.Append(Enum.GetName(typeof(AssetType), start));
            }
            else
            {
                sb.Append(comparandName);
                sb.Append(" >= AssetType.");
                sb.Append(Enum.GetName(typeof(AssetType), start));
                sb.Append(" && ");
                sb.Append(comparandName);
                sb.Append(" <= AssetType.");
                sb.Append(Enum.GetName(typeof(AssetType), end));
            }
        }

        return sb.ToString();
    }

    static string BuildParentCasts(Assets assets, string name)
    {
        var sb = new StringBuilder();
        foreach (var parent in assets.ParentsByAssetId[name])
        {
            sb.AppendLine($"    public static implicit operator {parent}({name} id) => {parent}.FromUInt32(id._value);");
            sb.AppendLine($"    public static explicit operator {name}({parent} id) => new {name}(id.ToUInt32());");
        }
        return sb.ToString();
    }

    static string BuildEnumCasts(string name, string[] enumNames)
    {
        var sb = new StringBuilder();
        foreach (var fullEnumName in enumNames.OrderBy(x => x))
        {
            int index = fullEnumName.IndexOf(',');
            string enumName = index == -1 ? fullEnumName : fullEnumName.Substring(0, index);
            sb.AppendLine($"    public static implicit operator {name}({enumName} id) => {name}.From(id);");
        }

        return sb.ToString();
    }

    static string BuildSimpleConstructor(string name, IList<AssetType> types)
    {
        return types.Count == 1 
            ? @$"
    public {name}(int id)
    {{
#if DEBUG
        if (id < 0 || id > 0xffffff)
            throw new ArgumentOutOfRangeException($""Tried to construct a {name} with out of range id {{id}}"");
#endif
        _value = (uint)AssetType.{types[0]} << 24 | (uint)id;
    }}
" : "";

    }

    static string FormatAssetTypeList(IList<AssetType> types)
    {
        var sb = new StringBuilder(types.Count * 30);
        int lineOffset = 0;
        bool first = true;
        foreach (var type in types)
        {
            if (!first)
                sb.Append(", ");

            if (sb.Length - lineOffset > 60)
            {
                sb.AppendLine();
                sb.Append("        ");
                lineOffset = sb.Length;
            }

            sb.Append("AssetType.");
            sb.Append(type);

            first = false;
        }
        if (lineOffset == 0)
            return sb.ToString();

        return Environment.NewLine + "        " + sb;
    }

    static string BuildClass(Assets assets, string destNamespace, string name, IList<AssetType> types)
    {
        if ((types?.Count ?? 0) == 0)
            throw new InvalidOperationException();

        bool single = types.Count == 1;
        string firstType = $"AssetType.{types[0]}";
        var condition = BuildIfCondition(types, "type");
        var condition2 = BuildIfCondition(types, "Type");

        string simpleConstructor = BuildSimpleConstructor(name, types);
        string familyCasts = "";
        string compoundCasts = BuildParentCasts(assets, name);
        string enumCasts = assets.EnumsByAssetId.ContainsKey(name) ? BuildEnumCasts(name, assets.EnumsByAssetId[name]) : "";
        string extras = "";
        if (assets.AssetIdConfig.Extras.TryGetValue(name, out var extraLines))
            extras = string.Join(Environment.NewLine, extraLines) + Environment.NewLine;

        // TODO: To...Id() methods for copy families.
        // TODO: Generation-time checks for copy family consistency
        // TODO: Implicit casts from single-type ids to compound ids (e.g. PartyMemberId -> SheetId)

        return $@"// Note: This file was automatically generated using Tools/CodeGenerator.
// No changes should be made to this file by hand. Instead, the relevant json
// files should be modified and then GenerateEnums should be used to regenerate
// the various types.
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace {destNamespace};

[JsonConverter(typeof(ToStringJsonConverter<{name}>))]
[TypeConverter(typeof({name}Converter))]
public readonly struct {name} : IEquatable<{name}>, IEquatable<AssetId>, IComparable, IAssetId
{{
    readonly uint _value;
    public {name}(AssetType type, int id = 0)
    {{
        if (!({condition}))
            throw new ArgumentOutOfRangeException($""Tried to construct a {name} with a type of {{type}}"");
#if DEBUG
        if (id < 0 || id > 0xffffff)
            throw new ArgumentOutOfRangeException($""Tried to construct a {name} with out of range id {{id}}"");
#endif
        _value = (uint)type << 24 | (uint)id;
    }}
{simpleConstructor}
    {name}(uint id) 
    {{
        _value = id;
        if (!({condition2}))
            throw new ArgumentOutOfRangeException($""Tried to construct a {name} with a type of {{Type}}"");
    }}

    public static {name} From<T>(T id) where T : unmanaged, Enum => ({name})AssetMapping.Global.EnumToId(id);

    public int ToDisk(AssetMapping mapping)
    {{
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
        return mapping.EnumToId(enumType, enumValue).Id;
    }}

    public static {name} FromDisk({(single ? "" : "AssetType type, ")}int disk, AssetMapping mapping)
    {{
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));{(single ? "" : $@"
        if (!({condition}))
            throw new ArgumentOutOfRangeException($""Tried to construct a {name} with a type of {{type}}"");
")}
        var (enumType, enumValue) = mapping.IdToEnum(new {name}({(single ? firstType : "type")}, disk));
        return ({name})AssetMapping.Global.EnumToId(enumType, enumValue);
    }}

    public static {name} SerdesU8(string name, {name} id, {(single ? "" : "AssetType type, ")}AssetMapping mapping, ISerializer s)
    {{
        if (s == null) throw new ArgumentNullException(nameof(s));

        byte diskValue = (byte)id.ToDisk(mapping);
        diskValue = s.UInt8(name, diskValue);
        id = FromDisk({(single ? "" : "type, ")}diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString(), true);
        return id;
    }}

    public static {name} SerdesU16(string name, {name} id, {(single ? "" : "AssetType type, ")}AssetMapping mapping, ISerializer s)
    {{
        if (s == null) throw new ArgumentNullException(nameof(s));

        ushort diskValue = (ushort)id.ToDisk(mapping);
        diskValue = s.UInt16(name, diskValue);
        id = FromDisk({(single ? "" : "type, ")}diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString(), true);
        return id;
    }}

    public static {name} SerdesU16BE(string name, {name} id, {(single ? "" : "AssetType type, ")}AssetMapping mapping, ISerializer s)
    {{
        if (s == null) throw new ArgumentNullException(nameof(s));

        ushort diskValue = (ushort)id.ToDisk(mapping);
        diskValue = s.UInt16BE(name, diskValue);
        id = FromDisk({(single ? "" : "type, ")}diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString(), true);
        return id;
    }}

    public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
    public readonly int Id => (int)(_value & 0xffffff);
    public static {name} None => new {name}(AssetType.None);
    public bool IsNone => Type == AssetType.None;

    public override string ToString() => AssetMapping.Global.IdToName(this);
    public string ToStringNumeric() => Id.ToString(CultureInfo.InvariantCulture);
    public static AssetType[] ValidTypes = {{ {FormatAssetTypeList(types)} }};
    public static {name} Parse(string s) => AssetMapping.Global.Parse(s, ValidTypes);

    public static implicit operator AssetId({name} id) => AssetId.FromUInt32(id._value);
    public static implicit operator {name}(AssetId id) => new {name}(id.ToUInt32());
{familyCasts}{compoundCasts}{enumCasts}
    public readonly int ToInt32() => unchecked((int)_value);
    public readonly uint ToUInt32() => _value;
    public static {name} FromInt32(int id) => new {name}(unchecked((uint)id));
    public static {name} FromUInt32(uint id) => new {name}(id);
    public static bool operator ==({name} x, {name} y) => x.Equals(y);
    public static bool operator !=({name} x, {name} y) => !(x == y);
    public static bool operator ==({name} x, AssetId y) => x.Equals(y);
    public static bool operator !=({name} x, AssetId y) => !(x == y);
    public static bool operator <({name} x, {name} y) => x.CompareTo(y) == -1;
    public static bool operator >({name} x, {name} y) => x.CompareTo(y) == 1;
    public static bool operator <=({name} x, {name} y) => x.CompareTo(y) != 1;
    public static bool operator >=({name} x, {name} y) => x.CompareTo(y) != -1;
    public bool Equals({name} other) => _value == other._value;
    public bool Equals(AssetId other) => _value == other.ToUInt32();
    public override bool Equals(object obj) => obj is IAssetId other && other.ToUInt32() == _value;
    public int CompareTo(object obj) => (obj is IAssetId other) ? _value.CompareTo(other.ToUInt32()) : -1;
    public override int GetHashCode() => unchecked((int)_value);
{extras}}}

public class {name}Converter : TypeConverter
{{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
        => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
        => value is string s ? {name}.Parse(s) : base.ConvertFrom(context, culture, value);

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        => destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
}}";
    }
}
