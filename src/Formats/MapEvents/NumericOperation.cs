using System;

namespace UAlbion.Formats.MapEvents;

public enum NumericOperation : byte
{
    SetToMinimum = 0,
    SetToMaximum = 1,
    Toggle = 2,
    SetAmount = 3,
    AddAmount = 4,
    SubtractAmount = 5,
    AddPercentage = 6,
    SubtractPercentage = 7
}

public static class NumericOperationExtensions
{
    public static ushort Apply16(this NumericOperation operation, ushort existing, ushort immediate, ushort min = 0, ushort max = ushort.MaxValue)
        => (ushort)operation.Apply(existing, immediate, min, max);

    public static int Apply(this NumericOperation operation, int existing, int immediate, int min = 0, int max = int.MaxValue)
    {
        var result = operation switch
        {
            NumericOperation.SetToMinimum => min,
            NumericOperation.SetToMaximum => max,
            NumericOperation.Toggle => existing,
            NumericOperation.SetAmount => immediate,
            NumericOperation.AddAmount => existing + immediate,
            NumericOperation.SubtractAmount => existing - immediate,
            NumericOperation.AddPercentage => existing + immediate * (max - min) / 100,
            NumericOperation.SubtractPercentage => existing - immediate * (max - min) / 100,
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };

        return Math.Clamp(result, min, max);
    }
}