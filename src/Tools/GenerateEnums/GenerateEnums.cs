using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Config;

namespace UAlbion.CodeGenerator
{
    public class EnumEntry
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public override string ToString() => $"{Name} = {Value}";
    }

    public class EnumData
    {
        public string FullName { get; set; }
        public string EnumType { get; set; }
        public AssetType AssetType { get; set; }
        public IList<EnumEntry> Entries { get; } = new List<EnumEntry>();
        public string Namespace => FullName.Substring(0, FullName.LastIndexOf('.'));
        public string TypeName => FullName.Substring(FullName.LastIndexOf('.') + 1);
        public string CopiedFrom { get; set; }
    }

    static class GenerateEnums
    {
        public static void Generate(Assets assets)
        {
            const string RelativeOutputPath = @"src/Base"; // TODO: Pull from asset config
            var outputPath = Path.Combine(assets.BaseDir, RelativeOutputPath);
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            // TODO: Extension methods from Base enums to AssetId, .ToId() or just .Id()?
            foreach (var e in assets.Enums.Values)
            {
                File.WriteAllText(Path.Combine(outputPath, e.TypeName + ".g.cs"),
$@"// Note: This file was automatically generated using Tools/GenerateEnums.
// No changes should be made to this file by hand. Instead, the relevant json
// files should be modified and then GenerateEnums should be used to regenerate
// the various ID enums.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace {e.Namespace}
{{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum {e.TypeName} {(e.EnumType != null ? ":" : "")} {e.EnumType}
    {{
" +
                string.Join(Environment.NewLine, e.Entries.OrderBy(x => x.Value).Select(x => $"        {x.Name} = {x.Value},"))
                + $@"
    }}
}}
#pragma warning restore CA1707 // Identifiers should not contain underscores
");
            }
        }
    }
}
