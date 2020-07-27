using System;
using System.Collections.Generic;
using SerdesNet;

namespace UAlbion.Formats.Assets.Flic
{
    public class Palette8Chunk : FlicChunk
    {
        public class PalettePacket
        {
            public byte Skip { get; private set; }
            public byte Copy { get; private set; }
            public byte[] Triplets { get; private set; }

            public static PalettePacket Serdes(int _, PalettePacket p, ISerializer s)
            {
                p ??= new PalettePacket();
                p.Skip = s.UInt8(nameof(Skip), p.Skip);
                p.Copy = s.UInt8(nameof(Copy), p.Copy);

                int count = p.Copy == 0 ? 256 : p.Copy;
                p.Triplets ??= new byte[3 * count];
                for (int i = 0; i < count; i++)
                {
                    p.Triplets[i * 3]     = s.UInt8("R", p.Triplets[i * 3]);
                    p.Triplets[i * 3 + 1] = s.UInt8("G", p.Triplets[i * 3 + 1]);
                    p.Triplets[i * 3 + 2] = s.UInt8("B", p.Triplets[i * 3 + 2]);
                }

                return p;
            }
        }

        public override FlicChunkType Type => FlicChunkType.Palette8Bit;
        public IList<PalettePacket> Packets { get; private set; }
        protected override uint SerdesBody(uint length, ISerializer s)
        {
            ushort packetCount = s.UInt16("PacketCount", (ushort)(Packets?.Count ?? 0));
            Packets = s.List(nameof(Packets), Packets, packetCount, PalettePacket.Serdes);
            return length;
        }

        public byte[] GetEffectivePalette(ReadOnlySpan<byte> existing)
        {
            byte[] palette = new byte[3 * 256];
            if(existing.Length != palette.Length)
                throw new InvalidOperationException($"Existing palette had invalid size {existing.Length}, expected {palette.Length}");

            for (int i = 0; i < palette.Length; i++)
                palette[i] = existing[i];

            foreach (var packet in Packets)
            {
                int i = packet.Skip;
                int count = packet.Copy == 0 ? 256 : packet.Copy;
                for (; i < packet.Skip + count; i++)
                {
                    palette[i * 3] = packet.Triplets[i * 3];
                    palette[i * 3 + 1] = packet.Triplets[i * 3 + 1];
                    palette[i * 3 + 2] = packet.Triplets[i * 3 + 2];
                }
            }

            return palette;
        }
    }
}