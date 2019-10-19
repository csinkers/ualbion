using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace UAlbion.Formats.Parsers
{
    public class AlbionStringTable
    {
        readonly IDictionary<int, string> _strings = new Dictionary<int, string>();

        string BytesTo850String(byte[] bytes)
        {
            return Encoding.GetEncoding(850).GetString(bytes).Replace("×", "ß").TrimEnd((char) 0);
        }

        public string this[int i] => _strings[i];

        public AlbionStringTable(BinaryReader br, long streamLength, StringTableType type = StringTableType.Lookup)
        {
            var startOffset = br.BaseStream.Position;
            switch (type)
            {
                case StringTableType.Lookup:
                {
                    var stringCount = br.ReadUInt16();
                    var stringLengths = new int[stringCount + 1];

                    for (int i = 1; i <= stringCount; i++)
                        stringLengths[i] = br.ReadUInt16();

                    for (int i = 1; i <= stringCount; i++)
                    {
                        var bytes = br.ReadBytes(stringLengths[i]);
                        _strings[i] = BytesTo850String(bytes);
                    }

                    break;
                }

                case StringTableType.SystemText:
                {
                    var fullText = BytesTo850String(br.ReadBytes((int)streamLength));
                    var lines = fullText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        if (line[0] != '[')
                            continue;
                        var untilColon = line.Substring(1, line.IndexOf(':') - 1);
                        int id = int.Parse(untilColon);
                        _strings[id] = line.Substring(line.IndexOf(':') + 1).TrimEnd(']');
                    }

                    break;
                }
                case StringTableType.ItemNames:
                {
                    const int stringSize = 20;
                    Debug.Assert(streamLength % stringSize == 0);
                    for (int i = 0; i < streamLength / stringSize; i++)
                    {
                        var bytes = br.ReadBytes(stringSize).Where(x => x != (char)0).ToArray();
                        _strings[i] = BytesTo850String(bytes);
                    }
                    break;
                }
            }
            Debug.Assert(br.BaseStream.Position == startOffset + streamLength);
        }
    }
}