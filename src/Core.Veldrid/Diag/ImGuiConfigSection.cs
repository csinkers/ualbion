using System;
using System.Collections.Generic;

namespace UAlbion.Core.Veldrid.Diag;

public class ImGuiConfigSection
{
    public ImGuiConfigSection(string name) => Name = name;
    public ImGuiConfigSection(string name, List<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        Name = name;
        foreach (var line in lines)
            Lines.Add(line);
    }

    public string Name { get; set; }
    public List<string> Lines { get; } = [];
    public override string ToString() => $"{Name} ({Lines.Count} lines)";
}