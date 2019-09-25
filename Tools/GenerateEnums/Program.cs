using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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
        public IList<EnumEntry> Entries { get; } = new List<EnumEntry>();
    }

    class Program
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
            var baseDir = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Parent.Parent.Parent.Parent.FullName;
            AssetConfig config = AssetConfig.Load(baseDir);
            var outpathPath = Path.Combine(baseDir, @"Formats/AssetIds");
            var xldPattern = new Regex(@"([0-9]+).XLD$");

            var enums = new Dictionary<string, EnumData>();
            foreach (var xld in config.Xlds)
            {
                if (string.IsNullOrEmpty(xld.Value.EnumName))
                    continue;

                int offset = 0;
                var match = xldPattern.Match(xld.Key);
                if (match.Success)
                    offset = 100 * int.Parse(match.Groups[1].Value);

                if (!enums.ContainsKey(xld.Value.EnumName))
                    enums[xld.Value.EnumName] = new EnumData { Name = xld.Value.EnumName };
                var e = enums[xld.Value.EnumName];

                foreach (var o in xld.Value.Assets)
                {
                    var id = offset + o.Key;
                    e.Entries.Add(string.IsNullOrEmpty(o.Value.Name)
                        ? new EnumEntry { Name = $"Unknown{id}", Value = id }
                        : new EnumEntry { Name = Sanitise(o.Value.Name), Value= id});
                }
            }

            ItemConfig itemConfig = ItemConfig.Load(baseDir);
            enums["ItemId"] = new EnumData { Name = "ItemId" };
            foreach (var item in itemConfig.Items)
                enums["ItemId"].Entries.Add(string.IsNullOrEmpty(item.Value.Name)
                    ? new EnumEntry { Name = $"Unknown{item.Key}", Value = item.Key }
                    : new EnumEntry { Name = Sanitise(item.Value.Name), Value= item.Key});

            CoreSpriteConfig coreSpriteConfig = CoreSpriteConfig.Load(baseDir);
            enums["CoreSpriteId"] = new EnumData { Name = "CoreSpriteId" };
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

                    o.Name = name +  counters[name];
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

namespace UAlbion.Formats.AssetIds
{{
    public enum {e.Name}
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
