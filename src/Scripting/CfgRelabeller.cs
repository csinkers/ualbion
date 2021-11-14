using System;
using System.Collections.Generic;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting
{
    public static class CfgRelabeller
    {
        class LabelCollectionAstVisitor : BaseAstVisitor
        {
            public List<string> Labels { get;  }= new();
            public override void Visit(Label label)
            {
                if (!Labels.Contains(label.Name))
                    Labels.Add(label.Name);
            }
        }

        class RelabellingAstVisitor : BaseBuilderAstVisitor
        {
            readonly IDictionary<string, string> _mapping;
            public RelabellingAstVisitor(IDictionary<string, string> mapping) => _mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
            public override ICfgNode Build(Goto jump) => Emit.Goto(_mapping[jump.Label]);
            public override ICfgNode Build(Label label) => Emit.Label(_mapping[label.Name]);
        }

        public static ControlFlowGraph Relabel(ControlFlowGraph graph)
        {
            var collector = new LabelCollectionAstVisitor();
            foreach (var index in graph.GetDfsOrder())
                graph.Nodes[index].Accept(collector);

            int i = 1;
            var mapping = new Dictionary<string, string>();
            foreach (var label in collector.Labels)
                mapping[label] = $"L{i++}";

            var applier = new RelabellingAstVisitor(mapping);
            var result = graph;
            foreach (var index in graph.GetDfsOrder())
            {
                result.Nodes[index].Accept(applier);
                if (applier.Result != null)
                    result = result.ReplaceNode(index, applier.Result);
            }

            return result;
        }
    }
}