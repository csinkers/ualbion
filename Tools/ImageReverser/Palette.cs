using System.IO;

namespace UAlbion.Tools.ImageReverser
{
    public class Palette
    {
        public readonly string Name;
        //public readonly ushort[] Entries = new ushort[0x120];
        public readonly byte[] Entries = new byte[0x240];

        private static readonly byte[] Special = {
            0,  0,0,
            4,  0,4,
            8,  0,8,
            12, 0,12,
            16, 0,16,
            20, 0,20,
            24, 0,24,
            28, 0,28,
            32, 0,32,
            36, 0,36,
            40, 0,40,
            44, 0,44,
            48, 0,48,
            52, 0,52,
            56, 0,56,
            60, 0,60,
            64, 0,64,
            68, 0,68,
            72, 0,72,
            76, 0,76,
            80, 0,80,
            85, 0,85,
            89, 0,89,
            93, 0,93,
            97, 0,97,
            101,0,101,
            105,0,105,
            109,0,109,
            113,0,113,
            117,0,117,
            121,0,121,
            125,0,125,
            129,0,129,
            133,0,133,
            137,0,137,
            141,0,141,
            145,0,145,
            149,0,149,
            153,0,153,
            157,0,157,
            161,0,161,
            165,0,165,
            170,0,170,
            174,0,174,
            178,0,178,
            182,0,182,
            186,0,186,
            190,0,190,
            194,0,194,
            198,0,198,
            202,0,202,
            206,0,206,
            210,0,210,
            214,0,214,
            218,0,218,
            222,0,222,
            226,0,226,
            230,0,230,
            234,0,234,
            238,0,238,
            242,0,242,
            246,0,246,
            250,0,250,
            255,0,255
        };

        static readonly byte[] FiveBitFixup = {
            0, 8, 16, 24, 32, 41, 49, 57, 65, 74, 82, 90, 98, 106, 115, 123, 131,
            139, 148, 156, 164, 172, 180, 189, 197, 205, 213, 222, 230, 238, 246, 255
        };

        static readonly byte[] SixBitFixup = {
            0, 4, 8, 12, 16, 20, 24, 28,
            32, 36, 40, 44, 48, 52, 56, 60,
            64, 68, 72, 76, 80, 85, 89, 93,
            97, 101, 105, 109, 113, 117, 121, 125,
            129, 133, 137, 141, 145, 149, 153, 157,
            161, 165, 170, 174, 178, 182, 186, 190,
            194, 198, 202, 206, 210, 214, 218, 222,
            226, 230, 234, 238, 242, 246, 250, 255
        };

        public Palette(string name)
        {
            Name = name;
        }

        public static Palette Load(string filename, string name)
        {
            using(var stream = File.OpenRead(filename))
            using (var br = new BinaryReader(stream))
            {
                var palette = new Palette(name);
                if (stream.Length == 0)
                {
                    for (int i = 0; i < 192; i++)
                    {
                        palette.Entries[i * 3] = (byte)i;
                        palette.Entries[i * 3 + 1] = (byte)i;
                        palette.Entries[i * 3 + 2] = (byte)i;
                        //var r = (int)((float) i / 255) * (2 << 5);
                        //var g = (int)((float) i / 255) * (2 << 6) << 5;
                        //var b = (int)((float) i / 255) * (2 << 5) << 11;
                        //palette.Entries[i] = (ushort)(r | g | b);
                    }

                    return palette;
                }

                for (int i = 0; i < palette.Entries.Length; i++)
                    //palette.Entries[i] = br.ReadUInt16();
                    palette.Entries[i] = br.ReadByte();
                return palette;
            }
        }
        public byte Red(byte colour)
        {
            if (colour >= 192)
                return Special[(colour - 192) * 3];
            return Entries[colour * 3];
        }

        public byte Green(byte colour)
        {
            if (colour >= 192)
                return Special[(colour - 192) * 3 + 1];
            return Entries[colour * 3 + 1];
        }
        public byte Blue(byte colour)
        {
            if (colour >= 192)
                return Special[(colour - 192) * 3 + 2];
            return Entries[colour * 3 + 2];
        }
/*
        public byte Red(byte colour)
        {
            var entry = Entries[colour + 0x20];
            return FiveBitFixup[entry & 0b00000000_00011111];
        }

        public byte Green(byte colour)
        {
            var entry = Entries[colour + 0x20];
            return SixBitFixup[entry & 0b00000111_11100000 >> 5];
        }
        public byte Blue(byte colour)
        {
            var entry = Entries[colour + 0x20];
            return FiveBitFixup[entry & 0b11111000_00000000 >> 11];
        }
*/
        public override string ToString() { return Name; }
    }
}
