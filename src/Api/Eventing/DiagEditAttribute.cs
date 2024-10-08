﻿using System;

namespace UAlbion.Api.Eventing;

/// <summary>
/// Allow editing this value in the diag inspector
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DiagEditAttribute : Attribute
{
    public DiagEditStyle Style { get; set; }
    public int MaxLength { get; set; } = int.MaxValue; // For strings
    public int Min { get; set; } = int.MaxValue; // For sliders
    public int Max { get; set; } = int.MinValue; // For sliders
    public string MinProperty { get; set; } // For sliders
    public string MaxProperty { get; set; } // For sliders
}