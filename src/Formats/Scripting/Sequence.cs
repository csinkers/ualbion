using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace UAlbion.Formats.Scripting
{
    public class Sequence : ICfgNode
    {
        public Sequence(IEnumerable<ICfgNode> nodes, ICfgNode newNode)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            if (newNode == null) throw new ArgumentNullException(nameof(newNode));
            var builder = ImmutableArray.CreateBuilder<ICfgNode>();
            builder.AddRange(nodes);
            builder.Add(newNode);
            Nodes = builder.ToImmutable();
        }

        public Sequence(params ICfgNode[] nodes) => Nodes = (nodes ?? throw new ArgumentNullException(nameof(nodes))).ToImmutableArray();
        public ImmutableArray<ICfgNode> Nodes { get; }
        public void ToPseudocode(StringBuilder sb, string indent, bool numeric = false)
        {
            foreach (var node in Nodes)
                node.ToPseudocode(sb, indent, numeric);
        }
    }
}