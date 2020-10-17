using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UAlbion.Config;

namespace UAlbion.CodeGenerator
{
    static class GenerateAssetIds
    {
        public static void Generate(Assets assets)
        {
            const string destNamespace = "UAlbion.Formats.Assets";
            const string destPath = "src/Formats/Assets";

            foreach (var assetType in assets.AssetIdConfig.Mappings)
            {
                var outputPath = Path.Combine(assets.BaseDir, destPath, assetType.Key + ".g.cs");
                var text = BuildClass(assets, destNamespace, assetType.Key, assetType.Value);
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
                sb.AppendLine($"        public static implicit operator {parent}({name} id) => new {parent}(id._value);");
                sb.AppendLine($"        public static explicit operator {name}({parent} id) => new {name}((uint)id);");
            }
            return sb.ToString();
        }

        static string BuildEnumCasts(string name, string[] enumNames)
        {
            var sb = new StringBuilder();
            foreach (var enumName in enumNames)
                sb.AppendLine($"        public static implicit operator {name}({enumName} id) => {name}.From(id);");
            return sb.ToString();
        }

        static string BuildClass(Assets assets, string destNamespace, string name, IList<AssetType> types)
        {
            if ((types?.Count ?? 0) == 0)
                throw new InvalidOperationException();

            bool single = types.Count == 1;
            string firstType = $"AssetType.{types[0]}";
            var condition = BuildIfCondition(types, "type");
            var condition2 = BuildIfCondition(types, "Type");

            string familyCasts = "";
            string compoundCasts = BuildParentCasts(assets, name);
            string enumCasts = assets.EnumsByAssetId.ContainsKey(name) ? BuildEnumCasts(name, assets.EnumsByAssetId[name]) : "";
            string extras = "";
            if (assets.AssetIdConfig.Extras.TryGetValue(name, out var extraLines))
                extras = string.Join(Environment.NewLine, extraLines) + Environment.NewLine;

            // TODO: To...Id() methods for copy families.
            // TODO: Generation-time checks for copy family consistency
            // TODO: Implicit casts from single-type ids to compound ids (e.g. PartyMemberId -> CharacterId)

            return $@"// Note: This file was automatically generated using Tools/GenerateEnums.
// No changes should be made to this file by hand. Instead, the relevant json
// files should be modified and then GenerateEnums should be used to regenerate
// the various types.
using System;
using Newtonsoft.Json;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace {destNamespace}
{{
    [JsonConverter(typeof(ToStringJsonConverter))]
    public struct {name} : IEquatable<{name}>, IEquatable<AssetId>, ITextureId
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

        public {name}(uint id) 
        {{ 
            _value = id;
            if (!({condition2}))
                throw new ArgumentOutOfRangeException($""Tried to construct a {name} with a type of {{Type}}"");
        }}
        public {name}(int id)
        {{
            _value = unchecked((uint)id);
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
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            {(single ? "" : $@"
            if (!({condition}))
                throw new ArgumentOutOfRangeException($""Tried to construct a {name} with a type of {{type}}"");")}

            var (enumType, enumValue) = mapping.IdToEnum(new {name}({(single ? firstType : "type")}, disk));
            return ({name})AssetMapping.Global.EnumToId(enumType, enumValue);
        }}

        public static {name} SerdesU8(string name, {name} id, {(single ? "" : "AssetType type, ")}AssetMapping mapping, ISerializer s)
        {{
            if (s == null) throw new ArgumentNullException(nameof(s));

            byte diskValue = (byte)id.ToDisk(mapping);
            diskValue = s.UInt8(name, diskValue);
            return FromDisk({(single ? "" : "type, ")}diskValue, mapping);
        }}

        public static {name} SerdesU16(string name, {name} id, {(single ? "" : "AssetType type, ")}AssetMapping mapping, ISerializer s)
        {{
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16(name, diskValue);
            return FromDisk({(single ? "" : "type, ")}diskValue, mapping);
        }}

        public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
        public readonly int Id => (int)(_value & 0xffffff);
        public static {name} None => new {name}(AssetType.None);
        public bool IsNone => Type == AssetType.None;

        public override string ToString() => AssetMapping.Global.IdToName(this);
        public static {name} Parse(string s)
        {{
            throw new NotImplementedException(); // TODO: Add proper parsing of arbitrary asset enums
            // if (s == null || !s.Contains("":""))
            //     throw new FormatException($""Tried to parse an InventoryId without a : (\""{{s}}\"")"");
            // var parts = s.Split(':');
            // //var type = (AssetType)Enum.Parse(typeof(AssetType), parts[0]);
            // var type = AssetTypeExtensions.FromShort(parts[0]);
            // var id = ushort.Parse(parts[1], CultureInfo.InvariantCulture);
            // return new {name}(type, id);
        }}

        public static implicit operator AssetId({name} id) => new AssetId(id._value);
        public static implicit operator {name}(AssetId id) => new {name}((uint)id);
        public static explicit operator uint({name} id) => id._value;
        public static explicit operator int({name} id) => unchecked((int)id._value);
        public static explicit operator {name}(int id) => new {name}(id);
{familyCasts}{compoundCasts}{enumCasts}
        public static {name} To{name}(int id) => new {name}(id);
        public readonly int ToInt32() => (int)this;
        public readonly uint ToUInt32() => (uint)this;
        public static bool operator ==({name} x, {name} y) => x.Equals(y);
        public static bool operator !=({name} x, {name} y) => !(x == y);
        public static bool operator ==({name} x, AssetId y) => x.Equals(y);
        public static bool operator !=({name} x, AssetId y) => !(x == y);
        public bool Equals({name} other) => _value == other._value;
        public bool Equals(AssetId other) => _value == (uint)other;
        public override bool Equals(object obj) => obj is ITextureId other && Equals(other);
        public override int GetHashCode() => (int)this;
{extras}    }}
}}";
        }
    }
}
