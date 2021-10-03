using System;
using System.Text;
using UAlbion.Api;

namespace UAlbion.Formats.Scripting
{
    public class Block : ICfgNode
    {
        public Block(IEvent[] events) => Events = events ?? throw new ArgumentNullException(nameof(events));
        IEvent[] Events { get; }
        public void ToPseudocode(StringBuilder sb, string indent, bool numeric)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            foreach (var e in Events)
            {
                sb.Append(indent);
                sb.AppendLine(numeric ? e.ToStringNumeric() : e.ToString());
            }
        }
    }
}