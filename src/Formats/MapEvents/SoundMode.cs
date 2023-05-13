using System.Diagnostics.CodeAnalysis;
namespace UAlbion.Formats.MapEvents;

[SuppressMessage("", "CA1027")]
public enum SoundMode : byte
{
    Silent = 0, // ??
    GlobalOneShot = 1,
    LocalLoop = 4
}