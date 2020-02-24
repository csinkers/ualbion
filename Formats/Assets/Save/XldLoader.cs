using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Assets.Save
{
    public class XldLoader // TODO: Combine this with XldFile at some point.
    {
        const string MagicString = "XLD0I";
        public static int HeaderSize(int itemCount) => MagicString.Length + 3 + 4 * itemCount;

        static int[] HeaderSerdes(int[] lengths, ISerializer s)
        {
            s.Check();
            string magic = s.NullTerminatedString("MagicString", MagicString);
            s.Check();
            if(magic != MagicString)
                throw new FormatException("XLD file magic string not found");

            ushort objectCount = s.UInt16("ObjectCount", (ushort)(lengths?.Length ?? 0));
            s.Check();
            lengths ??= new int[objectCount];

            for (int i = 0; i < objectCount; i++)
            {
                lengths[i] = s.Int32("Length" + i, lengths[i]);
                s.Check();
            }

            return lengths;
        }

        static int WithSerializer(SerializerMode mode, MemoryStream stream, Action<ISerializer> func, ISerializer parentSerializer, ref int fakeOffset)
        {
            switch (mode)
            {
                case SerializerMode.Writing:
                {
                    using var bw = new BinaryWriter(stream, Encoding.GetEncoding(850), true);
                    var s = new GenericBinaryWriter(bw);
                    func(s);
                    return (int)s.Offset;
                }

                case SerializerMode.WritingAnnotated:
                {
                    using var tw = new StreamWriter(stream, Encoding.GetEncoding(850), 1024, true);
                    var s = new AnnotatedFormatWriter(tw, (AnnotatedFormatWriter)parentSerializer);
                    s.Seek(fakeOffset);
                    func(s);
                    int length = (int)s.Offset - fakeOffset;
                    fakeOffset = (int)s.Offset;
                    return length;
                }

                default: throw new InvalidOperationException();
            }
        }

        public static void Serdes(XldCategory category, ushort xldNumber, ISerializer s, Action<int, int, ISerializer> func, IList<int> populatedIds)
        {
            s.Comment($"Xld{category}.{xldNumber}");
            s.Indent();

            if (s.Mode == SerializerMode.Reading)
            {
                var descriptor = s.Meta("XldDescriptor", (XldDescriptor)null, XldDescriptor.Serdes);
                Debug.Assert(descriptor.Category == category);
                Debug.Assert(xldNumber == descriptor.Number);

                var lengths = HeaderSerdes(null, s);

                Debug.Assert(lengths.Sum() + HeaderSize(lengths.Length) == descriptor.Size);
                long offset = s.Offset;
                for (int i = 0; i < 100 && i < lengths.Length; i++)
                {
                    if (lengths[i] == 0)
                        continue;
                    func(i + xldNumber * 100, lengths[i], s);
                    offset += lengths[i];
                    Debug.Assert(offset == s.Offset);
                }
            }
            else
            {
                int maxPopulatedId = populatedIds.Where(x => x >= xldNumber * 100 && x < (xldNumber + 1) * 100).Max() + 1 - xldNumber * 100;
                var buffers = Enumerable.Range(0, maxPopulatedId).Select(x => new MemoryStream()).ToArray();

                var descriptorOffset = s.Offset;
                s.Seek(s.Offset + 8);

                try
                {
                    var lengths = new int[maxPopulatedId];
                    int initialFakeOffset = (int)s.Offset + HeaderSize(maxPopulatedId);
                    int fakeOffset = initialFakeOffset;
                    for (int i = 0; i < maxPopulatedId; i++)
                        lengths[i] = WithSerializer(s.Mode, buffers[i], memorySerializer => func(i + xldNumber * 100, 0, memorySerializer), s, ref fakeOffset);

                    HeaderSerdes(lengths, s);
                    Debug.Assert(initialFakeOffset == s.Offset);
                    for (int i = 0; i < lengths.Length; i++)
                    {
                        if (s.Mode == SerializerMode.WritingAnnotated)
                        {
                            var content = Encoding.GetEncoding(850).GetString(buffers[i].ToArray());
                            s.Meta($"XLD{xldNumber}:{i}", content, (i,x,_) => s.NullTerminatedString($"XLD{xldNumber}:{i}", content));
                        }
                        else
                        {
                            s.ByteArray($"XLD{xldNumber}:{i}", buffers[i].ToArray(), (int) buffers[i].Length);
                        }
                    }

                    var endOffset = s.Offset;
                    s.Seek(descriptorOffset);

                    var descriptor = new XldDescriptor
                    {
                        Category = category,
                        Number = xldNumber,
                        Size = (uint)(lengths.Sum() + lengths.Length * 4 + 8)
                    };
                    s.Meta("Descriptor", descriptor, XldDescriptor.Serdes);

                    s.Seek(endOffset);
                }
                finally
                {
                    foreach (var buffer in buffers)
                        buffer.Dispose();
                }
            }
            s.Unindent();
        }
    }
}