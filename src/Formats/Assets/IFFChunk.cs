﻿using System;
using SerdesNet;

namespace UAlbion.Formats.Assets
{
    public class IFFChunk
    {
        public string TypeId { get; private set; }
        public int Length { get; private set; }
        long _lengthOffset;

        IFFChunk() { }
        public IFFChunk(string typeId, int length)
		{
			TypeId = typeId;
			Length = length;
		}

        public void WriteLength(ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            var offset = s.Offset;
            s.Seek(_lengthOffset);
            Length = s.Int32BE(nameof(Length), (int)(offset - _lengthOffset));
            s.Seek(offset);
        }

		public static IFFChunk Serdes(int _, IFFChunk c, ISerializer s)
		{
            if (s == null) throw new ArgumentNullException(nameof(s));
            c ??= new IFFChunk();
            c.TypeId = s.FixedLengthString(nameof(TypeId), c.TypeId, 4);
            c._lengthOffset = s.Offset;
            c.Length = s.Int32BE(nameof(Length), c.Length);
            return c;
		}
	}
}
