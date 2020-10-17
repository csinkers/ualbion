using System.ComponentModel;
using Xunit;
using UAlbion.Config;

namespace UAlbion.Formats.Tests
{
    public class AssetMappingTests : Component
    {
        public enum ZeroBasedByte : byte { Zero = 0, One, Two }
        public enum OneBasedByte : byte { One = 1, Two, Three }
        public enum ZeroBasedShort : ushort { Zero = 0, One, Two }
        public enum OneBasedShort : ushort { One = 1, Two, Three }
        public enum GapByteZero : byte { Zero = 0, One, Foo255 = 255 }
        public enum GapByteOne : byte { One = 1, Two, Foo255 = 255 }

        public AssetMappingTests()
        {
        }

        [Fact]
        void TestRegistration()
        {
            AssetMapping.GlobalIsThreadLocal = true;
            var m = AssetMapping.Global;
            m.RegisterAssetType(typeof(ZeroBasedByte), AssetType.Portrait);
            Assert.Equal(new AssetId(AssetType.Portrait, 0), m.EnumToId(ZeroBasedByte.Zero));
            Assert.Equal(new AssetId(AssetType.Portrait, 1), m.EnumToId(ZeroBasedByte.One));
            Assert.Equal(new AssetId(AssetType.Portrait, 2), m.EnumToId(ZeroBasedByte.Two));
            m.RegisterAssetType(typeof(OneBasedByte), AssetType.Portrait);
            Assert.Equal(new AssetId(AssetType.Portrait, 3), m.EnumToId(OneBasedByte.One));
            Assert.Equal(new AssetId(AssetType.Portrait, 4), m.EnumToId(OneBasedByte.Two));
            Assert.Equal(new AssetId(AssetType.Portrait, 5), m.EnumToId(OneBasedByte.Three));
        }
    }
}
