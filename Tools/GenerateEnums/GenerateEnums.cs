using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UAlbion.Formats;
using UAlbion.Formats.Config;

namespace GenerateEnums
{
    public class EnumEntry
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public override string ToString() => $"{Name} = {Value}";
    }

    public class EnumData
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public IList<EnumEntry> Entries { get; } = new List<EnumEntry>();
    }

    static class GenerateEnums
    {
        static readonly char[] ForbiddenCharacters = { ' ', '\t', '-', '(', ')', ',', '?', '.', '"' };
        static string Sanitise(string x)
        {
            var chars = new List<char>();
            bool capitaliseNext = true;
            foreach (var c in x)
            {
                if (c == '\'')
                    continue;

                if (!ForbiddenCharacters.Contains(c))
                {
                    chars.Add(capitaliseNext ? char.ToUpper(c) : c);
                    capitaliseNext = false;
                }
                else capitaliseNext = true;
            }

            return new string(chars.ToArray());
        }

        static void Main()
        {
            var baseDir = FormatUtil.FindBasePath();
            var config = FullAssetConfig.Load(baseDir);
            var outpathPath = Path.Combine(baseDir, @"Formats/AssetIds");
            var xldPattern = new Regex(@"([0-9]+).XLD$");

            var enums = new Dictionary<string, EnumData>();
            foreach (var xld in config.Xlds.Values)
            {
                if (string.IsNullOrEmpty(xld.EnumName))
                    continue;

                int offset = 0;
                var match = xldPattern.Match(xld.Name);
                if (match.Success)
                    offset = 100 * int.Parse(match.Groups[1].Value);

                if (!enums.ContainsKey(xld.EnumName))
                    enums[xld.EnumName] = new EnumData { Name = xld.EnumName, Type = xld.EnumType};
                var e = enums[xld.EnumName];

                foreach (var o in xld.Assets.Values.OrderBy(x => x.Id))
                {
                    var id = offset + o.Id;

                    if (e.Type == "byte" && id > 0xff)
                        continue;

                    e.Entries.Add(string.IsNullOrEmpty(o.Name)
                        ? new EnumEntry { Name = $"Unknown{id}", Value = id }
                        : new EnumEntry { Name = Sanitise(o.Name), Value = id });
                }
            }

            CoreSpriteConfig coreSpriteConfig = CoreSpriteConfig.Load(baseDir);
            enums["CoreSpriteId"] = new EnumData { Name = "CoreSpriteId", Type = "byte" };
            foreach (var item in coreSpriteConfig.CoreSpriteIds)
                enums["CoreSpriteId"].Entries.Add(new EnumEntry { Name = Sanitise(item.Value), Value = item.Key });

            foreach (var e in enums.Values)
            {
                var duplicateNames = e.Entries.GroupBy(x => x.Name).Where(x => x.Count() > 1).ToList();
                var counters = duplicateNames.ToDictionary(x => x.Key, x => 1);

                foreach (var o in e.Entries)
                {
                    if (!counters.ContainsKey(o.Name))
                        continue;
                    var name = o.Name;

                    int count = counters[name];
                    o.Name = count == 1 ? name : name + count;
                    counters[name]++;
                }
            }

            foreach (var e in enums.Values)
            {
                File.WriteAllText(Path.Combine(outpathPath, e.Name + ".cs"),
$@"// Note: This file was automatically generated using Tools/GenerateEnums.
// No changes should be made to this file by hand. Instead, the relevant json
// files should be modified and then GenerateEnums should be used to regenerate
// the various ID enums.

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace UAlbion.Formats.AssetIds
{{
    public enum {e.Name} {(e.Type != null ? ":" : "")} {e.Type}
    {{
" +
                string.Join(Environment.NewLine, e.Entries.Select(x => $"        {x.Name} = {x.Value},"))
                + @"
    }
}");
            }
        }
    }
}
