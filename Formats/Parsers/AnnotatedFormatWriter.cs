using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UAlbion.Formats.Parsers
{
    class AnnotatedFormatWriter : ISerializer
    {
        public AnnotatedFormatWriter(TextWriter textWriter)
        {
            tw = textWriter;
        }

        readonly TextWriter tw;
        int indent = 0;
        void doIndent() => tw.Write(new string(' ', indent));

        public SerializerMode Mode => SerializerMode.WritingAnnotated;
        public void Comment(string msg) { doIndent(); tw.WriteLine("// {0}", msg); }
        public void Indent() => indent += 4;
        public void Unindent() => indent -= 4;
        public void NewLine() => tw.WriteLine();
        public long Offset { get; private set; }

        public void Seek(long newOffset) { tw.WriteLine("Seek to {0} for overwrite", newOffset); Offset = newOffset; }

        public void Int8(string name, Func<sbyte> getter, Action<sbyte> setter)
        {
            doIndent();
            tw.WriteLine("{0:X} {1} = {2} (0x{2:X} y)", Offset, name, getter());
            Offset += 1L;
        }
        public void Int16(string name, Func<short> getter, Action<short> setter)
        {
            doIndent();
            tw.WriteLine("{0:X} {1} = {2} (0x{2:X} s)", Offset, name, getter());
            Offset += 2L;
        }
        public void Int32(string name, Func<int> getter, Action<int> setter)
        {
            doIndent();
            tw.WriteLine("{0:X} {1} = {2} (0x{2:X})", Offset, name, getter());
            Offset += 4L;
        }
        public void Int64(string name, Func<long> getter, Action<long> setter)
        {
            doIndent();
            tw.WriteLine("{0:X} {1} = {2} (0x{2:X} L)", Offset, name, getter());
            Offset += 8L;
        }

        public void UInt8(string name, Func<byte> getter, Action<byte> setter)
        {
            doIndent();
            tw.WriteLine("{0:X} {1} = {2} (0x{2:X} uy)", Offset, name, getter());
            Offset += 1L;
        }
        public void UInt16(string name, Func<ushort> getter, Action<ushort> setter)
        {
            doIndent();
            tw.WriteLine("{0:X} {1} = {2} (0x{2:X} us)", Offset, name, getter());
            Offset += 2L;
        }
        public void UInt32(string name, Func<uint> getter, Action<uint> setter)
        {
            doIndent();
            tw.WriteLine("{0:X} {1} = {2} (0x{2:X} u)", Offset, name, getter());
            Offset += 4L;
        }
        public void UInt64(string name, Func<ulong> getter, Action<ulong> setter)
        {
            var v = getter();
            doIndent();
            tw.WriteLine("{0:X} {1} = 0x{2:X}`{3:X8} UL ({4})", Offset, name, (v & 0xffffffff00000000UL) >> 32, v & 0xffffffffUL, v);
            Offset += 8L;
        }

        public void EnumU8<T>(string name, Func<T> getter, Action<T> setter, Func<T, (byte, string)> infoFunc) where T : Enum
        {
            var v = getter();
            var (value, label) = infoFunc(v);
            doIndent();
            tw.WriteLine("{0:X} {1} = {2} (0x{2:X} uy) // {3}", Offset, name, value, label);
            Offset += 1L;
        }

        public void EnumU16<T>(string name, Func<T> getter, Action<T> setter, Func<T, (ushort, string)> infoFunc) where T : Enum
        {
            var v = getter();
            var (value, label) = infoFunc(v);
            doIndent();
            tw.WriteLine("{0:X} {1} = {2} (0x{2:X} us) // {3}", Offset, name, value, label);
            Offset += 2L;
        }

        public void EnumU32<T>(string name, Func<T> getter, Action<T> setter, Func<T, (uint, string)> infoFunc) where T : Enum
        {
            var v = getter();
            var (value, label) = infoFunc(v);
            doIndent();
            tw.WriteLine("{0:X} {1} = {2} (0x{2:X} u) // {3}", Offset, name, value, label);
            Offset += 4L;
        }

        public void Guid(string name, Func<Guid> getter, Action<Guid> setter)
        {
            var v = getter();
            doIndent();
            tw.WriteLine("{0:X} {1} = {2:B}", Offset, name, v);
            Offset += 16L;
        }

        static readonly string[] hexStringTable =
        {
            "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "0A", "0B", "0C", "0D", "0E", "0F",
            "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "1A", "1B", "1C", "1D", "1E", "1F",
            "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "2A", "2B", "2C", "2D", "2E", "2F",
            "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "3A", "3B", "3C", "3D", "3E", "3F",
            "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "4A", "4B", "4C", "4D", "4E", "4F",
            "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "5A", "5B", "5C", "5D", "5E", "5F",
            "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "6A", "6B", "6C", "6D", "6E", "6F",
            "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "7A", "7B", "7C", "7D", "7E", "7F",
            "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "8A", "8B", "8C", "8D", "8E", "8F",
            "90", "91", "92", "93", "94", "95", "96", "97", "98", "99", "9A", "9B", "9C", "9D", "9E", "9F",
            "A0", "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8", "A9", "AA", "AB", "AC", "AD", "AE", "AF",
            "B0", "B1", "B2", "B3", "B4", "B5", "B6", "B7", "B8", "B9", "BA", "BB", "BC", "BD", "BE", "BF",
            "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "CA", "CB", "CC", "CD", "CE", "CF",
            "D0", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "DA", "DB", "DC", "DD", "DE", "DF",
            "E0", "E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8", "E9", "EA", "EB", "EC", "ED", "EE", "EF",
            "F0", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "FA", "FB", "FC", "FD", "FE", "FF"
        };

        static string ConvertToHexString(byte[] bytes)
        {
            var result = new System.Text.StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
                result.Append(hexStringTable[b]);
            return result.ToString();
        }

        public void ByteArray(string name, Func<byte[]> getter, Action<byte[]> setter, int n)
        {
            var v = getter();
            doIndent();
            tw.WriteLine("{0:X} {1} = {2}", Offset, name, ConvertToHexString(v));
            Offset += v.Length;
        }

        public void ByteArray2(string name, Func<byte[]> getter, Action<byte[]> setter, int n, string comment)
        {
            var v = getter();
            doIndent();
            tw.WriteLine("{0:X} {1} = {2}", Offset, name, comment);
            Offset += v.Length;
        }

        public void ByteArrayHex(string name, Func<byte[]> getter, Action<byte[]> setter, int n)
        {
            var v = getter();
            doIndent();
            tw.Write("{0:X} {1} = ", Offset, name);

            Indent();
            var payloadOffset = 0;
            var sb = new System.Text.StringBuilder(16);
            foreach (var b in v)
            {
                if (payloadOffset % 16 == 0)
                {
                    tw.Write(' ');
                    tw.Write(sb.ToString());
                    sb.Clear();
                    tw.WriteLine();
                    doIndent();
                    tw.Write("{0:X4}: ", payloadOffset);
                }
                else if (payloadOffset % 8 == 0) tw.Write('-');
                else if (payloadOffset % 2 == 0) tw.Write(' ');

                tw.Write("{0:X2}", b);

                if (b >= (byte)' ' && b <= 0x7e) sb.Append(Convert.ToChar(b));
                else sb.Append('.');

                payloadOffset += 1;
            }

            if (sb.Length > 0)
            {
                var missingChars = 16 - sb.Length;
                var spaceCount = missingChars * 2 + missingChars / 2 + 1;
                tw.Write(Enumerable.Repeat(' ', spaceCount));
                tw.Write(sb.ToString());
            }

            tw.WriteLine();
            Unindent();
            Offset += v.Length;
        }

        public void NullTerminatedString(string name, Func<string> getter, Action<string> setter)
        {
            var v = getter();
            doIndent();
            tw.Write("{0:X} {1} = {2}", Offset, name, v);

            var bytes = System.Text.Encoding.Unicode.GetBytes(v);
            Offset += bytes.Length + 2; // add 2 bytes for the null terminator
        }

        public void FixedLengthString(string name, Func<string> getter, Action<string> setter, int length)
        {
            var v = getter();
            doIndent();
            tw.Write("{0:X} {1} = {2}", Offset, name, v);

            var bytes = System.Text.Encoding.Unicode.GetBytes(v);
            if (bytes.Length > length + 2) throw new InvalidOperationException("Tried to write overlength string");

            Offset += length; // add 2 bytes for the null terminator
        }

        public void RepeatU8(string name, byte v, int length)
        {
            doIndent();
            tw.WriteLine(
                "{0:X} {1} = [{2} bytes (0x{2:X}) of 0x{3:X}]",
                Offset,
                name,
                length,
                v
            );
            Offset += length;
        }

        public void Meta(string name, Action<ISerializer> serializer, Action<ISerializer> deserializer)
        {
            indent += 4;
            doIndent();
            tw.WriteLine("// {0}", name);
            serializer(this);
            indent -= 4;
        }
        public void Check() { }

        public void Dynamic<TTarget>(TTarget target, string propertyName)
        {
            var serializer = SerializationInfo.Get<TTarget>(propertyName);
            switch (serializer)
            {
                case SerializationInfo<TTarget, byte>   s: UInt8( s.Name, () => s.Getter(target), x => s.Setter(target, x)); break;
                case SerializationInfo<TTarget, sbyte>  s:  Int8( s.Name, () => s.Getter(target), x => s.Setter(target, x)); break;
                case SerializationInfo<TTarget, ushort> s: UInt16(s.Name, () => s.Getter(target), x => s.Setter(target, x)); break;
                case SerializationInfo<TTarget, short>  s:  Int16(s.Name, () => s.Getter(target), x => s.Setter(target, x)); break;
                case SerializationInfo<TTarget, uint>   s: UInt32(s.Name, () => s.Getter(target), x => s.Setter(target, x)); break;
                case SerializationInfo<TTarget, int>    s:  Int32(s.Name, () => s.Getter(target), x => s.Setter(target, x)); break;
                case SerializationInfo<TTarget, ulong>  s: UInt64(s.Name, () => s.Getter(target), x => s.Setter(target, x)); break;
                case SerializationInfo<TTarget, long>   s:  Int64(s.Name, () => s.Getter(target), x => s.Setter(target, x)); break;
                default: throw new InvalidOperationException($"Tried to serialize unexpected type {serializer.Type}");
            }
        }

        public void List<TTarget>(IList<TTarget> list, int count, Action<TTarget, ISerializer> serializer, Func<TTarget> constructor)
        {
            indent += 4;
            doIndent();
            tw.Write("[ ");
            for (int i = 0; i < count; i++)
            {
                var og = list[i];
                serializer(og, this);
            }
            tw.Write(" ]");
            indent -= 4;
        }
    }
}
