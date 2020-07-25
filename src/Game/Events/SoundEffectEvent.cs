using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("sound_effect")]
    public class SoundEffectEvent : GameEvent
    {
        public SoundEffectEvent(int soundId, int unk1, int unk2, int unk3, int unk4) { SoundId = soundId; Unk1 = unk1; Unk2 = unk2; Unk3 = unk3; Unk4 = unk4; }
        [EventPart("soundId ")] public int SoundId { get; }
        [EventPart("unk1 ")] public int Unk1 { get; }
        [EventPart("unk2 ")] public int Unk2 { get; }
        [EventPart("unk3 ")] public int Unk3 { get; }
        [EventPart("unk4")] public int Unk4 { get; }
    }
}
