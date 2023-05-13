using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UAlbion.Api;
using UAlbion.Config;

namespace DumpEnums;

public static class Program
{
    public static void Main()
    {
        var disk = new FileSystem(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        var baseDir = ConfigUtil.FindBasePath(disk);
        var enumsDir = Path.Combine(baseDir, @"src/Base");
        var outputDir = @"D:\Depot\AlbionRE\scripts";

        foreach (var file in disk.EnumerateDirectory(enumsDir, "*.cs"))
        {
            var lines = disk.ReadAllLines(file);
            var enums = Collect(ParseLines(lines));
            foreach (var e in enums)
            {
                var sb = new StringBuilder();
                sb.AppendLine($@"## ###
#  Import Albion {e.Name} enum
##
# Import Albion {e.Name} enum

#@category Albion.IdEnums

import math
from ghidra.app.util.datatype import DataTypeSelectionDialog
from ghidra.framework.plugintool import PluginTool
from ghidra.util.data.DataTypeParser import AllowedDataTypes
from ghidra.program.model.data import *

dtm = currentProgram.getDataTypeManager()
catPath = CategoryPath(""/Albion/Ids"")
category = dtm.getCategory(catPath)
if (category == None):
    print(""Could not open category"")
else:");

                int size = e.BackingType switch
                {
                    "byte" => 1,
                    "short" => 2,
                    "ushort" => 2,
                    _ => 4
                };

                if (size == 1)
                    WriteEnum(sb, e, 1);

                if (size <= 2)
                    WriteEnum(sb, e, 2);

                WriteEnum(sb, e, 4);

                var result = sb.ToString();
                var outputPath = Path.Combine(outputDir, $"Update{e.Name}Id.py");
                disk.WriteAllText(outputPath, result);
            }
        }
    }

    static void WriteEnum(StringBuilder sb, EnumDef e, int size)
    {
        var name = e.Name + "Id" + size;
        sb.AppendLine($"    e = EnumDataType(category.categoryPath, \"{name}\", {size})");

        foreach (var entry in e.Entries)
            sb.AppendLine($"    e.add(\"{entry.Name}\", {entry.Value})");

        sb.AppendLine($@"
    existing = category.getDataType(""{name}"")
    if (existing == None):
        dtm.addDataType(e, None)
    else:
        existing.replaceWith(e)
");
    }

    record EnumHeader(string Name, string BackingType);
    record EnumEntry(string Name, int Value);
    record EnumDef(string Name, string BackingType, List<EnumEntry> Entries);

    static readonly Regex EnumRegex = new(@"^(?<access>public|private|internal)? enum (?<name>[^ ]+)(\s*:\s*(?<backingType>[^ ]+))?");
    static readonly Regex EntryRegex = new(@"^\s*(?<name>[^ ]+)\s*=\s*(?<value>\d+)\s*,?");

    static IEnumerable<object> ParseLines(IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            var m = EnumRegex.Match(line);
            if (m.Success)
            {
                yield return new EnumHeader(m.Groups["name"].Value, m.Groups["backingType"].Value);
                continue;
            }

            m = EntryRegex.Match(line);
            if (m.Success)
                yield return new EnumEntry(m.Groups["name"].Value, int.Parse(m.Groups["value"].Value));
        }
    }

    static IEnumerable<EnumDef> Collect(IEnumerable<object> lines)
    {
        EnumDef? current = null;
        foreach (var line in lines)
        {
            if (line is EnumHeader header)
            {
                if (current != null)
                    yield return current;

                current = new EnumDef(header.Name, header.BackingType, new List<EnumEntry>());
            }

            if (line is EnumEntry entry && current != null)
                current.Entries.Add(entry);
        }

        if (current != null)
            yield return current;
    }
}
