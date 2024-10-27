using System;
using SerdesNet;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets.Sheets;

#pragma warning disable CA1711
public class CharacterAttribute : ICharacterAttribute
{
    public ushort Current { get; set; }
    public ushort Max { get; set; }
    public ushort Boost { get; set; }
    public ushort Backup { get; set; }
    public CharacterAttribute DeepClone() => (CharacterAttribute)MemberwiseClone();
    public override string ToString() => $"[{Current}/{Max}]{(Boost > 0 ? $"+{Boost}" : "")}{(Backup > 0 ? $" (was {Backup})" : "")}";

    public static CharacterAttribute Serdes(string name, CharacterAttribute attr, ISerializer s, bool hasBackup = true)
    {
        ArgumentNullException.ThrowIfNull(s);
        s.Begin(name);
        attr ??= new CharacterAttribute();
        attr.Current = s.UInt16(nameof(Current), attr.Current);
        attr.Max = s.UInt16(nameof(Max), attr.Max);
        attr.Boost = s.UInt16(nameof(Boost), attr.Boost);
        if (hasBackup)
            attr.Backup = s.UInt16(nameof(Backup), attr.Backup);
        s.End();
        return attr;
    }

    public void Apply(NumericOperation operation, ushort amount)
    {
        Current = operation.Apply16(Current, amount, 0, Max);
    }

    public void ApplyToMax(NumericOperation operation, ushort amount)
    {
        Max = operation.Apply16(Max, amount);
        if (Current > Max)
            Current = Max;
    }
}