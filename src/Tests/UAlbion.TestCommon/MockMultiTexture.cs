using System;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Textures;

namespace UAlbion.TestCommon
{
    public class MockMultiTexture : MultiTexture
    {
        public MockMultiTexture(ITextureId id, string name, IPaletteManager paletteManager) : base(id, name, paletteManager)
        {
        }

        public override uint FormatSize => 1;

        public override void SavePng(int logicalId, int tick, string path)
        {
        }

        public void RebuildAll()
        {
            if (IsMetadataDirty)
                RebuildLayers();

            var palette = PaletteManager.Palette.GetCompletePalette();

            Span<uint> toBuffer = stackalloc uint[(int)(Width * Height)];
            foreach (var lsi in LogicalSubImages)
            {
                for (int i = 0; i < lsi.Frames; i++)
                {
                    toBuffer.Fill(lsi.IsAlphaTested ? 0 : 0xff000000);
                    Rebuild(lsi, i, toBuffer, palette);
                }
            }

            IsDirty = false;
        }
    }
}