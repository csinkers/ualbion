using System;

namespace UAlbion.Api.Eventing;

/// <summary>
/// Allow editing this value in the diag inspector
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class DiagEditAttribute : Attribute
{
    public DiagEditStyle Style { get; set; }
    public int? MaxLength { get; set; } // For strings
    public int? Min { get; set; } // For numeric types
    public int? Max { get; set; } // For numeric types
}