using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UAlbion.Formats.Scripting
{
    public class DebugVisualizerGraphData
    {
        [JsonPropertyName("kind")] public Dictionary<string, bool> Kind => Tags.ToDictionary(tag => tag, _ => true);
        [JsonIgnore] public string[] Tags => new[] { "graph" };
        [JsonPropertyName("nodes")] public List<DebugVisualizerNodeData> Nodes { get; } = new(); 
        [JsonPropertyName("edges")] public List<DebugVisualizerEdgeData> Edges { get; } = new(); 

        public static DebugVisualizerGraphData FromCfg(ControlFlowGraph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            var data = new DebugVisualizerGraphData();
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                var node = graph.Nodes[i];
                if (node == null)
                    continue;

                data.Nodes.Add(new DebugVisualizerNodeData(i.ToString(CultureInfo.InvariantCulture))
                {
                    Label = node.ToPseudocode()
                    // Color = ...
                });
            }

            foreach (var (start, end) in graph.Edges)
            {
                var label = graph.GetEdgeLabel(start, end);
                data.Edges.Add(new DebugVisualizerEdgeData(
                        start.ToString(CultureInfo.InvariantCulture),
                        end.ToString(CultureInfo.InvariantCulture))
                {
                    Label = label ? null : "f"
                });
            }

            return data;
        }

        public DebugVisualizerGraphData AddPointer(string name, int target)
        {
            Nodes.Add(new DebugVisualizerNodeData(name) { Color = "#30a030" });
            Edges.Add(new DebugVisualizerEdgeData(name, target.ToString(CultureInfo.InvariantCulture)));
            return this;
        }

        public override string ToString() => JsonSerializer.Serialize(this,
            new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                WriteIndented = true,
            });
    }
}
