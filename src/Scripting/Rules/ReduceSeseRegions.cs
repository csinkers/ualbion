using System;
using System.Linq;

namespace UAlbion.Scripting.Rules
{
    public static class ReduceSeseRegions
    {
        const string Description = "Reduce SESE region";
        public static (ControlFlowGraph, string) Decompile(ControlFlowGraph graph/*, RecordFunc recordFunc = null*/)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            var regions = graph.GetAllSeseRegions();

            // Do smallest regions first, as they may be nested in a larger one
            foreach (var (region, regionEntry, regionExit) in regions.OrderBy(x => x.nodes.Count))
            {
                if (regionEntry == graph.EntryIndex)
                    continue; // Don't try and reduce the 'sequence' of start node -> actual entry point nodes when there's only one entry

                /* Func<string> vis = () => // For VS Code debug visualisation
                {
                     var d = graph.ToVis(); 
                     foreach(var n in d.Nodes)
                        if (region.Contains(int.Parse(n.Id, CultureInfo.InvariantCulture)))
                            n.Color = "#4040b0";
                     return d.ToString();
                }; */
                bool containsOther = regions.Any(x => x.nodes != region && !x.nodes.Except(region).Any());
                if (containsOther)
                    continue;

                var cut = graph.Cut(region, regionEntry, regionExit);

                if (cut.Cut.IsCyclic()) // Loop reduction comes later
                    continue;

                return (cut.Merge(SeseReducer.Reduce(cut.Cut)), Description);
            }

            return (graph, null);
        }
    }
}