﻿using System;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.MapEvents;

public class UnparsableEvent : IVerboseEvent // No-op event for parsing failures in script files.
{
    public UnparsableEvent(string rawEventText) => RawEventText = rawEventText;
    public string RawEventText { get; }
    public override string ToString() => RawEventText;
    public void Format(IScriptBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Add(ScriptPartType.Error, RawEventText);
    }
}