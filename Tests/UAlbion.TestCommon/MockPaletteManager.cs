﻿using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Textures;

namespace UAlbion.TestCommon
{
    public class MockPaletteManager : ServiceComponent<IPaletteManager>, IPaletteManager
    {
        int _frame;

        public IPalette Palette { get; set; }
        public PaletteTexture PaletteTexture { get; set; }
        public int Version { get; private set; }

        public int Frame
        {
            get => _frame;
            set
            {
                _frame = value;
                Version++;
            }
        }
    }
}
