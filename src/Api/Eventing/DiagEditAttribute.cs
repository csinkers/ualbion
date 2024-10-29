using System;

namespace UAlbion.Api.Eventing;

/// <summary>
/// Allow editing this value in the diag inspector
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DiagEditAttribute : Attribute
{
    public DiagEditStyle Style { get; set; }
    public int MaxLength { get; set; } = int.MaxValue; // For strings
    public object Min { get; set; }
    public object Max { get; set; }
    public string MinProperty { get; set; } // For sliders
    public string MaxProperty { get; set; } // For sliders

    public Func<object, object> GetMinProperty { get; set; }
    public Func<object, object> GetMaxProperty { get; set; }
}