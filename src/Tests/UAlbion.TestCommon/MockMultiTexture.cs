﻿using System;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;

namespace UAlbion.TestCommon
{
    public class MockMultiTexture : MultiTexture
    {
        public MockMultiTexture(IAssetId id, string name, IPaletteManager paletteManager) : base(id, name, paletteManager)
        {
        }

        public override int FormatSize => 1;

        public override void SavePng(int logicalId, int tick, string path, IFileSystem disk)
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