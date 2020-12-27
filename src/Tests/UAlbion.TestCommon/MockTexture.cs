using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Core.Textures;

namespace UAlbion.TestCommon
{
    public class MockTexture : EightBitTexture
    {
        public MockTexture(
            ITextureId id,
            string name, 
            uint width, uint height,
            byte[] pixels, IEnumerable<SubImage> subImages) 
            : base(id, name, width, height, 0, 1, pixels, subImages)
        {
        }

        public MockTexture(
            ITextureId id,
            string name, 
            uint width, uint height,
            uint mipLevels, uint arrayLayers,
            byte[] pixels, IEnumerable<SubImage> subImages)
            : base(id, name, width, height, mipLevels, arrayLayers, pixels, subImages)
        {
        }
    }
}