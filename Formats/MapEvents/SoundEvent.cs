using System.Diagnostics;
using System.IO;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    public class SoundEvent : IEvent
    {
        public static EventNode Load(BinaryReader br, int id, MapEventType type)
        {
            var e = new SoundEvent
            {
                Mode = (SoundMode) br.ReadByte(), // +1
                SoundId = br.ReadByte(), // +2 // TODO: SoundId
                Unk3 = br.ReadByte(), // +3
                Volume = br.ReadByte(), // +4
                RestartProbability = br.ReadByte(), // +5
                FrequencyOverride = br.ReadUInt16(), // +6
                Unk8 = br.ReadUInt16(), // +8
            };
            Debug.Assert(e.Unk3 <= 100);
            Debug.Assert(e.Volume <= 150);
            Debug.Assert(e.RestartProbability <= 102);
            return new EventNode(id, e);
        }

        public enum SoundMode
        {
            Silent = 0, // ??
            GlobalOneShot = 1,
            LocalLoop = 4
        }

        public SoundMode Mode { get; private set; }
        public byte SoundId { get; private set; }
        public byte Unk3 { get; private set; } // 0 - 100?
        public byte Volume { get; private set; } // 0 - 150
        public byte RestartProbability { get; private set; } // 0 - 100
        public ushort FrequencyOverride { get; private set; }
        public ushort Unk8 { get; private set; }
        public override string ToString() => $"sound {SoundId} {Mode} Vol:{Volume} Prob:{RestartProbability}% Freq:{FrequencyOverride} ({Unk3} {Unk8})";
    }
}
