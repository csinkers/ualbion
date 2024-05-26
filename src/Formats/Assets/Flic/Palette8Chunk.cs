using System;
using SerdesNet;

namespace UAlbion.Formats.Assets.Flic;

public class Palette8Chunk : FlicChunk
{
    PalettePacket[] _packets;
    sealed class PalettePacket
    {
        public byte Skip { get; }
        public byte[] Triplets { get; }

        public PalettePacket(ISerializer s)
        {
            ArgumentNullException.ThrowIfNull(s);
            Skip = s.UInt8(null, 0);
            var copy = s.UInt8(null, 0);

            int count = copy == 0 ? 256 : copy;
            count *= 3;
            Triplets = new byte[count];
            for (int i = 0; i < count;)
            {
                Triplets[i++] = s.UInt8(null, 0);
                Triplets[i++] = s.UInt8(null, 0);
                Triplets[i++] = s.UInt8(null, 0);
            }
        }
    }

    public override FlicChunkType Type => FlicChunkType.Palette8Bit;
    protected override uint LoadChunk(uint length, ISerializer s)
    {
        ArgumentNullException.ThrowIfNull(s);
        ushort packetCount = s.UInt16(null, 0);
        _packets = new PalettePacket[packetCount];
        for(int i = 0; i < packetCount; i++)
            _packets[i] = new PalettePacket(s);
        return length;
    }

    public uint[] GetEffectivePalette(ReadOnlySpan<uint> existing)
    {
        uint[] palette = new uint[256];
        if(existing.Length != palette.Length)
            throw new InvalidOperationException($"Existing palette had invalid size {existing.Length}, expected {palette.Length}");

        for (int i = 0; i < palette.Length; i++)
            palette[i] = existing[i];

        foreach (var packet in _packets)
        {
            int final = (packet.Skip + packet.Triplets.Length / 3);
            int j = packet.Skip * 3;
            for (int i = packet.Skip; i < final; i++)
            {
                palette[i] =
                    (uint) 0xff << 24 // Alpha
                    | (uint) packet.Triplets[j++] // Red
                    | (uint) packet.Triplets[j++] << 8 // Green
                    | (uint) packet.Triplets[j++] << 16 // Blue
                    ;
            }
        }

        palette[0] = 0; // Index 0 should always be transparent
        return palette;
    }
}