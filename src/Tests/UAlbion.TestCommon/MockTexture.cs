using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Core.Textures;

namespace UAlbion.TestCommon
{
    public class MockTexture : EightBitTexture
    {
        public MockTexture(
            IAssetId id,
            string name, 
            int width, int height,
            byte[] pixels, IEnumerable<SubImage> subImages) 
            : base(id, name, width, height, 0, 1, pixels, subImages)
        {
        }

        public MockTexture(
            IAssetId id,
            string name, 
            int width, int height,
            int mipLevels, int arrayLayers,
            byte[] pixels, IEnumerable<SubImage> subImages)
            : base(id, name, width, height, mipLevels, arrayLayers, pixels, subImages)
        {
        }
    }
}