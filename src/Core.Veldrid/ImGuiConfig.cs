using System;
using System.Collections.Generic;
using System.Text;

namespace UAlbion.Core.Veldrid;

public class ImGuiConfig
{
    public List<ImGuiConfigSection> Sections { get; } = new();
    public static ImGuiConfig Load(string raw) => new(raw ?? "");

    public ImGuiConfig() { }

    public override string ToString()
    {
        var sb = new StringBuilder();

        bool first = true;
        foreach (var section in Sections)
        {
            if (!first)
                sb.AppendLine();

            sb.AppendLine(section.Name);
            foreach (var line in section.Lines)
                sb.AppendLine(line);
            first = false;
        }

        return sb.ToString();
    }

    ImGuiConfig(string raw)
    {
        var lines = raw.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        ImGuiConfigSection section = null;

        foreach (var line in lines)
        {
            if (line.StartsWith('['))
            {
                section = new ImGuiConfigSection(line);
                Sections.Add(section);
            }
            else
                section?.Lines.Add(line);
        }
    }
}
