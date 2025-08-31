using System;
using ImGuiColorTextEditNet;
using ImGuiColorTextEditNet.Syntax;

namespace UAlbion.Game.Veldrid.Diag;

public class AlbionSyntaxHighlighter : ISyntaxHighlighter
{
    static readonly object DefaultState = new();
    public bool AutoIndentation => true;
    public int MaxLinesPerFrame => 500;
    public string GetTooltip(string id) => null;
    public object Colorize(Span<Glyph> line, object state) => DefaultState;
}