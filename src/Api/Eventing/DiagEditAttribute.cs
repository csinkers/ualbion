using System;
using System.Text.Json.Serialization;

namespace UAlbion.Api.Eventing;

/// <summary>
/// Allow editing this value in the diag inspector
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DiagEditAttribute : Attribute
{
    public const int DefaultMaxStringLength = 1024;

    public DiagEditStyle Style { get; set; }
    public int MaxLength { get; set; } // For strings, 0 = no max
    public object Min { get; set; }
    public object Max { get; set; }
    public string MinProperty { get; set; } // For sliders
    public string MaxProperty { get; set; } // For sliders

    [JsonIgnore] public Func<object, object> GetMinProperty { get; set; }
    [JsonIgnore] public Func<object, object> GetMaxProperty { get; set; }
    [JsonIgnore] public override object TypeId => base.TypeId;

    public DiagEditAttribute Clone() => new()
    {
        Style = Style,
        MaxLength = MaxLength,
        Min = Min,
        Max = Max,
        MinProperty = MinProperty,
        MaxProperty = MaxProperty,
        GetMinProperty = GetMinProperty,
        GetMaxProperty = GetMaxProperty
    };

    public bool IsEquivalentTo(DiagEditAttribute other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Style == other.Style
               && MaxLength == other.MaxLength
               && Equals(Min, other.Min)
               && Equals(Max, other.Max)
               && MinProperty == other.MinProperty
               && MaxProperty == other.MaxProperty
               && Equals(GetMinProperty, other.GetMinProperty)
               && Equals(GetMaxProperty, other.GetMaxProperty);
    }
}
