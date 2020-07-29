using System;
using System.Collections.Generic;
using System.IO;

namespace UAlbion.Formats.Assets.Flic
{
    public class Palette8Chunk : FlicChunk
    {
        public class PalettePacket
        {
            public byte Skip { get; }
            public byte[] Triplets { get; }

            public PalettePacket(BinaryReader br)
            {
                Skip = br.ReadByte();
                var copy = br.ReadByte();

                int count = copy == 0 ? 256 : copy;
                count *= 3;
                Triplets = new byte[count];
                for (int i = 0; i < count;)
                {
                    Triplets[i++] = br.ReadByte();
                    Triplets[i++] = br.ReadByte();
                    Triplets[i++] = br.ReadByte();
                }
            }
        }

        public override FlicChunkType Type => FlicChunkType.Palette8Bit;
        public IList<PalettePacket> Packets { get; } = new List<PalettePacket>();
        protected override uint LoadChunk(uint length, BinaryReader br)
        {
            ushort packetCount = br.ReadUInt16();
            for(int i = 0; i < packetCount; i++)
                Packets.Add(new PalettePacket(br));
            return length;
        }

        public uint[] GetEffectivePalette(ReadOnlySpan<uint> existing)
        {
            uint[] palette = new uint[256];
            if(existing.Length != palette.Length)
                throw new InvalidOperationException($"Existing palette had invalid size {existing.Length}, expected {palette.Length}");

            for (int i = 0; i < palette.Length; i++)
                palette[i] = existing[i];

            foreach (var packet in Packets)
            {
                int final = (packet.Skip + packet.Triplets.Length / 3);
                int j = packet.Skip * 3;
                for (int i = packet.Skip; i < final; i++)
                {
                    palette[i] =
                          (uint) 0xff << 24 // Alpha
                        | (uint) packet.Triplets[j++] << 16 // Red
                        | (uint) packet.Triplets[j++] << 8 // Green
                        | (uint) packet.Triplets[j++] // Blue
                        ;
                }
            }

            return palette;
        }
    }
}