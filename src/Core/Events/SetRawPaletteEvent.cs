﻿using System;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

public class LoadRawPaletteEvent : EngineEvent, IVerboseEvent
{
    public string Name { get; }
    public uint[] Entries { get; }

    public LoadRawPaletteEvent(string name, uint[] entries)
    {
        Name = name;
        Entries = entries ?? throw new ArgumentNullException(nameof(entries));
    }
}