using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UAlbion.Scripting;

public class DebugVisualizerGraphData
{
#pragma warning disable CA1822 // Mark members as static
    [JsonPropertyName("kind")] public Dictionary<string, bool> Kind => Tags.ToDictionary(tag => tag, _ => true);
    [JsonIgnore] public string[] Tags => new[] { "graph" };
    [JsonPropertyName("nodes")] public List<DebugVisualizerNodeData> Nodes { get; } = new();
    [JsonPropertyName("edges")] public List<DebugVisualizerEdgeData> Edges { get; } = new();
#pragma warning restore CA1822 // Mark members as static

    static readonly JsonSerializerOptions Options = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        WriteIndented = true,
    };

    public static DebugVisualizerGraphData FromCfg(ControlFlowGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);
        var data = new DebugVisualizerGraphData();
        for (int i = 0; i < graph.Nodes.Count; i++)
        {
            var node = graph.Nodes[i];
            if (node == null)
                continue;

            data.Nodes.Add(new DebugVisualizerNodeData(i.ToString())
            {
                Label = node.ToString()
                // Color = ...
            });
        }

        foreach (var (start, end) in graph.Edges)
        {
            var label = graph.GetEdgeLabel(start, end);
            data.Edges.Add(new DebugVisualizerEdgeData(
                start.ToString(),
                end.ToString())
            {
                Label = label switch
                {
                    CfgEdge.True => null,
                    CfgEdge.False => "f",
                    CfgEdge.DisjointGraphFixup => "d",
                    _ => "?"
                }
            });
        }

        return data;
    }

    public DebugVisualizerGraphData AddPointer(string name, int target)
    {
        Nodes.Add(new DebugVisualizerNodeData(name) { Color = "#30a030" });
        Edges.Add(new DebugVisualizerEdgeData(name, target.ToString()));
        return this;
    }

    public override string ToString() => JsonSerializer.Serialize(this, Options);
}
