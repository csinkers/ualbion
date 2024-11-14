﻿using System;
using System.Collections.Generic;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting.Rules;

public static class SimplifyLabels
{
    public static (ControlFlowGraph result, string description) Apply(ControlFlowGraph graph) 
        => (Relabel(graph, ScriptConstants.DummyLabelPrefix), "Relabel");

    sealed class LabelCollectionAstVisitor : BaseAstVisitor
    {
        public Dictionary<string, ICfgNode> Labels { get; } = [];
        public override void Visit(Sequence seq)
        {
            List<string> labels = null;
            foreach (var statement in seq.Statements)
            {
                if (statement is Label label)
                {
                    labels ??= [];
                    labels.Add(label.Name);
                }
                else if (labels != null)
                {
                    foreach (var name in labels)
                        Labels[name] = statement;

                    labels = null;
                    statement.Accept(this);
                }
                else statement.Accept(this);
            }
        }
    }

    sealed class RelabellingAstVisitor : BaseAstBuilderVisitor
    {
        readonly IDictionary<string, (string target, bool removed)> _mapping;
        public RelabellingAstVisitor(IDictionary<string, (string target, bool removed)> mapping)
            => _mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
        protected override ICfgNode Build(GotoStatement jump) => 
            _mapping.TryGetValue(jump.Label, out var tuple) 
                ? Emit.Goto(tuple.target) 
                : null;

        protected override ICfgNode Build(Label label)
        {
            if (!_mapping.TryGetValue(label.Name, out var value)) 
                return null;

            return value.removed 
                ? Emit.Empty() 
                : Emit.Label(value.target);
        }
    }

    public static ControlFlowGraph Relabel(ControlFlowGraph graph, string dummyLabelPrefix)
    {
        ArgumentNullException.ThrowIfNull(graph);
        var collector = new LabelCollectionAstVisitor();
        graph.Accept(collector);

        int i = 1;
        var mapping = new Dictionary<string, (string target, bool removed)>();
        var targets = new Dictionary<ICfgNode, string>();

        // Remove non-dummy labels
        foreach (var (label, target) in collector.Labels)
        {
            if (!label.StartsWith(dummyLabelPrefix, StringComparison.Ordinal))
                continue;

            if (targets.TryGetValue(target, out var existingLabel))
            {
                mapping[label] = (existingLabel, true);
            }
            else
            {
                var newLabel = $"L{i++}";
                mapping[label] = (newLabel, false);
                targets[target] = newLabel;
            }
        }

        return graph.AcceptBuilder(new RelabellingAstVisitor(mapping));
    }
}