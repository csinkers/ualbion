using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Assets
{
    public class SavedGame
    {
        public string Name { get; private set; }
        public ushort Version { get; set; }
        public ushort Days { get; set; }
        public ushort Hours { get; set; }
        public ushort Minutes { get; set; }
        public MapDataId MapId { get; set; }
        public ushort PartyX { get; set; }
        public ushort PartyY { get; set; }

        public IDictionary<PartyCharacterId, CharacterSheet> PartyMembers { get; } = new Dictionary<PartyCharacterId, CharacterSheet>();
        public IDictionary<NpcCharacterId, CharacterSheet> Npcs { get; } = new Dictionary<NpcCharacterId, CharacterSheet>();
        public IDictionary<AutoMapId, byte[]> Automaps { get; } = new Dictionary<AutoMapId, byte[]>();
        public IDictionary<ChestId, Chest> Chests { get; } = new Dictionary<ChestId, Chest>();
        public IDictionary<MerchantId, Chest> Merchants { get; } = new Dictionary<MerchantId, Chest>();

        public static SavedGame Serdes(SavedGame save, ISerializer s)
        {
            save ??= new SavedGame();

            ushort nameLength = s.UInt16("NameLength", (ushort)(save.Name?.Length ?? 0));
            save.Unk0 = s.UInt16(nameof(Unk0), save.Unk0);
            save.Name = s.FixedLengthString(nameof(Name), save.Name, nameLength);

            save.Unk1 = s.UInt32(nameof(Unk1), save.Unk1);
            var versionOffset = s.Offset;
            save.Version = s.UInt16(nameof(Version), save.Version); // 0
            Debug.Assert(save.Version == 138); // TODO: Throw error for other versions?
            save.Unk9 = s.ByteArray(nameof(Unk9), save.Unk9, 6); // 2

            save.Days = s.UInt16(nameof(Days), save.Days); // 8
            save.Hours = s.UInt16(nameof(Hours), save.Hours); // A
            save.Minutes = s.UInt16(nameof(Minutes), save.Minutes); // C
            save.MapId = s.EnumU16(nameof(MapId), save.MapId); // E
            save.PartyX = s.UInt16(nameof(PartyX), save.PartyX); // 10
            save.PartyY = s.UInt16(nameof(PartyY), save.PartyY); // 12
            save.Unk14 = s.UInt16(nameof(Unk14), save.Unk14); // 14
            save.Unknown = s.ByteArray(nameof(Unknown), save.Unknown, (int)(0x947C + versionOffset - s.Offset));

            save.Mystery8 = s.Meta(nameof(Mystery8), save.Mystery8, MysteryChunk8.Serdes);
            save.Mystery8_2 = s.Meta(nameof(Mystery8_2), save.Mystery8_2, MysteryChunk8.Serdes);
            save.Mystery6 = s.Meta(nameof(Mystery6), save.Mystery6, MysteryChunk6.Serdes);

            // ASCII: XLD0I
            // Hex: 58, 4c, 44, 30, 49
            // Dec: 88, 76, 68, 48, 73

            /* XLDs start @ Version + 0x9556
             PRTCHAR0..2    (3)     1, 2, 3
             AUTOMAP1..3    (3)     4, 5, 6
             CHESTDT0,1,2,5 (4)  7, 8, 9,10
             MERCHDT0..2    (3)    11,12,13
             NPCCHAR0..2    (3)    14,15,16
                     Total: 16
            */

            var charLoader = new CharacterSheetLoader();
            var chestLoader = new ChestLoader();
            void SerdesPartyCharacter(int i, ISerializer serializer)
            {
                var key = (PartyCharacterId)i;
                CharacterSheet existing = null;
                if (serializer.Mode == SerializerMode.Reading || save.PartyMembers.TryGetValue(key, out existing))
                    save.PartyMembers[key] = charLoader.Serdes(existing, serializer, key.ToString(), null);
            }

            void SerdesNpcCharacter(int i, ISerializer serializer)
            {
                var key = (NpcCharacterId)i;
                CharacterSheet existing = null;
                if (serializer.Mode == SerializerMode.Reading || save.Npcs.TryGetValue(key, out existing))
                    save.Npcs[key] = charLoader.Serdes(existing, serializer, key.ToString(), null);
            }

            void SerdesAutomap(int i, ISerializer serializer)
            {
            }

            void SerdesChest(int i, ISerializer serializer)
            {
                var key = (ChestId)i;
                Chest existing = null;
                if (serializer.Mode == SerializerMode.Reading || save.Chests.TryGetValue(key, out existing))
                    save.Chests[key] = chestLoader.Serdes(existing, serializer, key.ToString(), null);
            }

            void SerdesMerchant(int i, ISerializer serializer)
            {
                var key = (MerchantId)i;
                Chest existing = null;
                if (serializer.Mode == SerializerMode.Reading || save.Merchants.TryGetValue(key, out existing))
                    save.Merchants[key] = chestLoader.Serdes(existing, serializer, key.ToString(), null);
            }

            SerdesXld(0, s, SerdesPartyCharacter);
            SerdesXld(1, s, SerdesPartyCharacter);
            SerdesXld(2, s, SerdesPartyCharacter);

            SerdesXld(1, s, SerdesAutomap);
            SerdesXld(2, s, SerdesAutomap);
            SerdesXld(3, s, SerdesAutomap);

            SerdesXld(0, s, SerdesChest);
            SerdesXld(1, s, SerdesChest);
            SerdesXld(2, s, SerdesChest);
            SerdesXld(5, s, SerdesChest);

            SerdesXld(0, s, SerdesMerchant);
            SerdesXld(1, s, SerdesMerchant);
            SerdesXld(2, s, SerdesMerchant);

            SerdesXld(0, s, SerdesNpcCharacter);
            SerdesXld(1, s, SerdesNpcCharacter);
            SerdesXld(2, s, SerdesNpcCharacter);

            // Pad?

            return save;
        }

        static void WithSerializer(SerializerMode mode, MemoryStream stream, Action<ISerializer> func)
        {
            if(mode == SerializerMode.Writing)
            {
                using var bw = new BinaryWriter(stream);
                var s = new GenericBinaryWriter(bw);
                func(s);
            }
            else if (mode == SerializerMode.WritingAnnotated)
            {
                using var tw = new StreamWriter(stream);
                var s = new AnnotatedFormatWriter(tw);
                func(s);
            }
        }

        static void SerdesXld(int xldNumber, ISerializer s, Action<int, ISerializer> func)
        {
            var xldLoader = new XldLoader();
            var descriptorOffset = s.Offset; // TODO: Write the descriptors
            if (s.Mode != SerializerMode.Reading)
            {
                s.Seek(s.Offset + 8);
            }
            else
            {
                var descriptor = s.Meta("XldDescriptor", (XldDescriptor)null, XldDescriptor.Serdes);
                Console.WriteLine($"XLD: {descriptor.Category}.{descriptor.Number}: {descriptor.Size} bytes");
            }

            if (s.Mode == SerializerMode.Reading)
            {
                var lengths = xldLoader.Serdes(null, s);
                long offset = s.Offset;
                for (int i = 0; i < 100 && i < lengths.Length; i++)
                {
                    if (lengths[i] == 0)
                        continue;
                    func(i + xldNumber * 100, s);
                    offset += lengths[i];
                    Debug.Assert(offset == s.Offset);
                }
            }
            else
            {
                var buffers = Enumerable.Range(0, 100).Select(x => new MemoryStream()).ToArray();
                try
                {
                    for (int i = 0; i < 100; i++)
                        WithSerializer(s.Mode, buffers[i], memorySerializer => func(i + xldNumber * 100, memorySerializer));

                    var lengths = 
                        buffers
                        .Select(x => (int)x.Length)
                        .Reverse() // Trim any zero-length resources at the end
                        .SkipWhile(x => x == 0)
                        .Reverse()
                        .ToArray();

                    xldLoader.Serdes(lengths, s);
                    for (int i = 0; i < lengths.Length; i++)
                        s.ByteArray($"XLD{xldNumber}:{i}", buffers[i].ToArray(), (int)buffers[i].Length);
                }
                finally
                {
                    foreach(var buffer in buffers)
                        buffer.Dispose();
                }
            }
        }

        public ushort Unk0 { get; set; }
        public uint Unk1 { get; set; }
        public byte[] Unk9 { get; set; }
        public ushort Unk14 { get; set; }
        public byte[] Unknown { get; set; }
        public MysteryChunk8 Mystery8 { get; set; }
        public MysteryChunk8 Mystery8_2 { get; set; }
        public MysteryChunk6 Mystery6 { get; set; }
    }

    public class XldDescriptor
    {
        public enum XldCategory : ushort
        {
            PartyCharacter = 17,
            Automap = 27,
        }

        public uint Size { get; set; }
        public XldCategory Category { get; set; }
        public ushort Number { get; set; }

        public static XldDescriptor Serdes(int _, XldDescriptor d, ISerializer s)
        {
            d ??= new XldDescriptor();
            d.Size = s.UInt32(nameof(Size), d.Size);
            d.Category = s.EnumU16(nameof(Category), d.Category);
            d.Number = s.UInt16(nameof(Number), d.Number);
            return d;
        }
    }

    public class MysteryChunk8
    {
        public enum ChunkType
        {
            MysterySmall = 0x3,
            Xld = 0x11,
            Mystery6Byte = 0xc8,
        }

        public uint Size { get; set; }
        public ushort NumChunks { get; set; }
        public UnkEightByte[] Contents { get; set; }

        public static MysteryChunk8 Serdes(int _, MysteryChunk8 c, ISerializer s)
        {
            c ??= new MysteryChunk8();
            c.Size = s.UInt32(nameof(Size), c.Size);
            c.NumChunks = s.UInt16(nameof(NumChunks), c.NumChunks);
            Debug.Assert(c.NumChunks == c.Size / 8);
            c.Contents ??= new UnkEightByte[(c.Size - 2) / 8];
            for (int i = 0; i < c.Contents.Length; i++)
                c.Contents[i] = UnkEightByte.Serdes(c.Contents[i], s);

            return c;
        }
    }

    public class MysteryChunk6
    {
        public uint Size { get; set; }
        public ushort NumChunks { get; set; }
        public UnkSixByte[] Contents { get; set; }

        public static MysteryChunk6 Serdes(int _, MysteryChunk6 c, ISerializer s)
        {
            c ??= new MysteryChunk6();
            c.Size = s.UInt32(nameof(Size), c.Size);
            c.NumChunks = s.UInt16(nameof(NumChunks), c.NumChunks);
            Debug.Assert(c.NumChunks == c.Size / 6);
            c.Contents ??= new UnkSixByte[(c.Size - 2) / 6];
            for (int i = 0; i < c.Contents.Length; i++)
                c.Contents[i] = UnkSixByte.Serdes(c.Contents[i], s);
            return c;
        }
    }

    public class UnkSixByte
    {
        public byte Unk0 { get; set; }
        public byte Unk1 { get; set; }
        public byte Unk2 { get; set; }
        public byte Unk3 { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }

        public override string ToString() => $"Unk {Unk0:X2} {Unk1:X2} {Unk2:X2} {Unk3:X2} {Unk4:X2} {Unk5:X2}";
        public static UnkSixByte Serdes(UnkSixByte u, ISerializer s)
        {
            u ??= new UnkSixByte();
            u.Unk0 = s.UInt8(nameof(Unk0), u.Unk0);
            u.Unk1 = s.UInt8(nameof(Unk1), u.Unk1);
            u.Unk2 = s.UInt8(nameof(Unk2), u.Unk2);
            u.Unk3 = s.UInt8(nameof(Unk3), u.Unk3);
            u.Unk4 = s.UInt8(nameof(Unk4), u.Unk4);
            u.Unk5 = s.UInt8(nameof(Unk5), u.Unk5);
            return u;
        }
    }

    public class UnkEightByte
    {
        public enum EBEnum1 : byte
        {
            EB1_Unk0,
            EB1_Unk1,
            EB1_Unk2,
            EB1_Unk3,
            EB1_Unk4,
            EB1_Unk5,
            EB1_Unk6,
            EB1_Unk7,
            EB1_Unk8,
            EB1_Unk9,
        }

        public enum EBEnum2 : byte
        {
            Common,
            Rare1,
            Rare2,
            Norm,
        }

        public byte X { get; set; } // Broad, mildly lower-biased distribution. Max below 255 (~220)
        public byte Y { get; set; } // Broad, mildly lower-biased distribution. Max below 255 (~220)
        public EBEnum1 Unk2 { get; set; } // Roughly equal distribution over [0..10], most likely an enum.
        public EBEnum2 Unk3 { get; set; } // Ranges over [0..3], 3 very popular, 0 moderately, 1 and 2 ~1% each.
        public ushort Underlay { get; set; }
        public ushort Overlay { get; set; }

        public override string ToString() => $"Unk {X:X2} {Y:X2} {Unk2:X2} {Unk3:X2} {Underlay:X6} {Overlay:X6}";
        public static UnkEightByte Serdes(UnkEightByte u, ISerializer s)
        {
            u ??= new UnkEightByte();
            u.X = s.UInt8(nameof(X), u.X);
            u.Y = s.UInt8(nameof(Y), u.Y);
            u.Unk2 = s.EnumU8(nameof(Unk2), u.Unk2);
            u.Unk3 = s.EnumU8(nameof(Unk3), u.Unk3);
            u.Underlay = s.UInt16(nameof(Underlay), u.Underlay);
            u.Overlay = s.UInt16(nameof(Overlay), u.Overlay);
            return u;
        }
    }
}
