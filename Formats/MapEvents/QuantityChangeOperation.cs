using System;

namespace UAlbion.Formats.MapEvents
{
    public enum QuantityChangeOperation : byte
    {
        SetToMinimum = 0,
        SetToMaximum = 1,
        Unk2 = 2,
        SetAmount = 3,
        AddAmount = 4,
        SubtractAmount = 5,
        AddPercentage = 6,
        SubtractPercentage = 7
    }

    public static class QuantityChangeOperationExtensions
    {
        public static int Apply(this QuantityChangeOperation operation, int existing, int immediate, int min, int max) =>
            operation switch
            {
                QuantityChangeOperation.SetToMinimum => min,
                QuantityChangeOperation.SetToMaximum => max,
                QuantityChangeOperation.Unk2 => existing,
                QuantityChangeOperation.SetAmount => immediate,
                QuantityChangeOperation.AddAmount => existing + immediate,
                QuantityChangeOperation.SubtractAmount => existing - immediate,
                QuantityChangeOperation.AddPercentage => existing + immediate * (max - min) / 100,
                QuantityChangeOperation.SubtractPercentage => existing - immediate * (max - min) / 100,
                _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
            };
    }
}
