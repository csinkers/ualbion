﻿using UAlbion.Api.Eventing;

namespace UAlbion.Scripting.Ast;

public record SingleEvent(IEvent Event, int OriginalIndex) : ICfgNode
{
    public override string ToString() => Event.ToString();
    public void Accept(IAstVisitor visitor) => visitor?.Visit(this);
    public int Priority => int.MaxValue;
}