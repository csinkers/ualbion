using UAlbion.Api;

namespace UAlbion.Formats.ScriptEvents;

[Event("start_anim")] // USED IN SCRIPT
public class StartAnimEvent : Event
{
    public StartAnimEvent(int unk1, int unk2, int unk3, int unk4, int? unk5) { Unk1 = unk1; Unk2 = unk2; Unk3 = unk3; Unk4 = unk4; Unk5 = unk5; }
    [EventPart("unk1 ")] public int Unk1 { get; }
    [EventPart("unk2 ")] public int Unk2 { get; }
    [EventPart("unk3 ")] public int Unk3 { get; }
    [EventPart("unk4 ")] public int Unk4 { get; }
    [EventPart("unk5", true)] public int? Unk5 { get; }
}