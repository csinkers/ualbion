using System.Collections.Generic;
using UAlbion.Core.Textures;

namespace UAlbion.TestCommon
{
    public class MockTexture : EightBitTexture
    {
        public MockTexture(
            string name, 
            uint width, uint height,
            byte[] pixels, IEnumerable<SubImage> subImages) 
            : base(name, width, height, 0, 1, pixels, subImages)
        {
        }

        public MockTexture(
            string name, 
            uint width, uint height,
            uint mipLevels, uint arrayLayers,
            byte[] pixels, IEnumerable<SubImage> subImages)
            : base(name, width, height, mipLevels, arrayLayers, pixels, subImages)
        {
        }
    }
}