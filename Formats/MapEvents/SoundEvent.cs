using System.Diagnostics;
using System.IO;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class SoundEvent : IMapEvent
    {
        public static SoundEvent Serdes(SoundEvent e, ISerializer s)
        {
            e ??= new SoundEvent();
            e.Mode = s.EnumU8(nameof(Mode), e.Mode);
            s.Dynamic(e, nameof(SoundId));
            s.Dynamic(e, nameof(Unk3));
            s.Dynamic(e, nameof(Volume));
            s.Dynamic(e, nameof(RestartProbability));
            s.Dynamic(e, nameof(FrequencyOverride));
            s.Dynamic(e, nameof(Unk8));
            Debug.Assert(e.Unk3 <= 100);
            Debug.Assert(e.Volume <= 150);
            Debug.Assert(e.RestartProbability <= 102);
            Debug.Assert(e.Unk8 == 0);
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
