using System.Diagnostics;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class SoundEvent : MapEvent
    {
        public SoundEvent(BinaryReader br, int id, EventType type) : base(id, type)
        {
            Mode = br.ReadByte(); // +1
            SoundId = br.ReadByte(); // +2
            Unk3 = br.ReadByte(); // +3
            Debug.Assert(Unk3 <= 100);
            Volume = br.ReadByte(); // +4
            Debug.Assert(Volume <= 150);
            RestartProbability = br.ReadByte(); // +5
            Debug.Assert(RestartProbability <= 100);
            FrequencyOverride = br.ReadUInt16(); // +6
            Unk8 = br.ReadUInt16(); // +8
        }

        public enum SoundMode
        {
            Silent = 0, // ??
            GlobalOneShot = 1,
            LocalLoop = 4
        }

        public byte Mode { get; }
        public byte SoundId { get; }
        public byte Unk3 { get; } // 0 - 100?
        public byte Volume { get; } // 0 - 150
        public byte RestartProbability { get; } // 0 - 100
        public ushort FrequencyOverride { get; }
        public ushort Unk8 { get; }
    }
}