﻿namespace UAlbion.Scripting.Ast;

public record Goto(string Label) : ICfgNode
{
    public override string ToString() => $"Goto({Label})";
    public void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public int Priority => int.MaxValue;
}