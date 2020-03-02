using UAlbion.Api;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class SoundEvent : Event, IMapEvent
    {
        public static SoundEvent Serdes(SoundEvent e, ISerializer s)
        {
            e ??= new SoundEvent();
            e.Mode = s.EnumU8(nameof(Mode), e.Mode);
            e.SoundId = s.UInt8(nameof(SoundId), e.SoundId);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Volume = s.UInt8(nameof(Volume), e.Volume);
            e.RestartProbability = s.UInt8(nameof(RestartProbability), e.RestartProbability);
            e.FrequencyOverride = s.UInt16(nameof(FrequencyOverride), e.FrequencyOverride);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            ApiUtil.Assert(e.Unk3 <= 100);
            ApiUtil.Assert(e.Volume <= 150);
            ApiUtil.Assert(e.RestartProbability <= 102);
            ApiUtil.Assert(e.Unk8 == 0);
            return e;
        }

        public enum SoundMode : byte
        {
            Silent = 0, // ??
            GlobalOneShot = 1,
            LocalLoop = 4
        }

        public SoundMode Mode { get; private set; }
        public byte SoundId { get; private set; } // [0..78], 153
        public byte Unk3 { get; private set; } // [0..100] (multiples of 5)
        public byte Volume { get; private set; } // [0..150]
        public byte RestartProbability { get; private set; } // [0..100]
        public ushort FrequencyOverride { get; private set; } // 0,8, [5..22]*1000
        ushort Unk8 { get; set; }
        public override string ToString() => $"sound {SoundId} {Mode} Vol:{Volume} Prob:{RestartProbability}% Freq:{FrequencyOverride} ({Unk3})";
        public MapEventType EventType => MapEventType.Sound;
    }
}
