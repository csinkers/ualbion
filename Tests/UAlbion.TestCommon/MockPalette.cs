﻿using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;

namespace UAlbion.TestCommon
{
    public class MockPalette : IPalette
    {
        readonly uint[] _entries = Enumerable.Range(0, 256).Select(x =>
            (uint)x | 
            (uint)x << 8 |
            (uint)x << 16 |
            (uint)(x == 0 ? 0 : 0xff) << 24
        ).ToArray();

        public int Id => 0;
        public string Name => "Mock";
        public IList<uint[]> GetCompletePalette() => new[] { _entries };
        public bool IsAnimated => false;
        public uint[] GetPaletteAtTime(int paletteFrame) => _entries;
    }
}