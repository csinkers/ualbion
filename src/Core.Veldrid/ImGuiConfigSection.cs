using System.Collections.Generic;

namespace UAlbion.Core.Veldrid;

public class ImGuiConfigSection
{
    public ImGuiConfigSection(string name) => Name = name;
    public ImGuiConfigSection(string name, List<string> lines)
    {
        Name = name;
        foreach (var line in lines)
            Lines.Add(line);
    }

    public string Name { get; set; }
    public List<string> Lines { get; } = new();
}