﻿using System;
using System.Text;

namespace UAlbion.Formats.Scripting
{
    public class BreakNode : ICfgNode
    {
        public override string ToString() => ((ICfgNode)this).ToPseudocode();
        public void ToPseudocode(StringBuilder sb, bool isStatement, bool numeric)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            sb.Append("break; ");
        }
    }
}