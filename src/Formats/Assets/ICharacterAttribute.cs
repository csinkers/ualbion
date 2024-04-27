namespace UAlbion.Formats.Assets;

#pragma warning disable CA1711
public interface ICharacterAttribute
{
    ushort Current { get; }
    ushort Max { get; }
    ushort Boost { get; }
    ushort Backup { get; }
}