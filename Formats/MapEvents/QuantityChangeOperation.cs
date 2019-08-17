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
}