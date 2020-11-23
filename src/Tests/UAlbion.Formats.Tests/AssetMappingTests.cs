using System;
using System.ComponentModel;
using Newtonsoft.Json;
using Xunit;
using UAlbion.Config;

namespace UAlbion.Formats.Tests
{
    public class AssetMappingTests : Component
    {
        enum ZeroBasedByte : byte { Zero = 0, One, Two }
        enum OneBasedByte : byte { One = 1, Two, Three }
        enum ZeroBasedShort : ushort { Zero = 0, One, Two }
        enum GapByteZero : byte { Zero = 0, One, Foo255 = 255 }
        enum GapByteOne : byte { One = 1, Two, Foo255 = 255 }

        public AssetMappingTests()
        {
            AssetMapping.GlobalIsThreadLocal = true;
        }

        [Fact]
        public void TestRegistration()
        {
            var m = AssetMapping.Global.Clear();
            m.RegisterAssetType(typeof(ZeroBasedByte), AssetType.Portrait);
            Assert.Equal(new AssetId(AssetType.Portrait, 0), m.EnumToId(ZeroBasedByte.Zero));
            Assert.Equal(new AssetId(AssetType.Portrait, 1), m.EnumToId(ZeroBasedByte.One));
            Assert.Equal(new AssetId(AssetType.Portrait, 2), m.EnumToId(ZeroBasedByte.Two));
            m.RegisterAssetType(typeof(OneBasedByte), AssetType.Portrait);
            Assert.Equal(new AssetId(AssetType.Portrait, 3), m.EnumToId(OneBasedByte.One));
            Assert.Equal(new AssetId(AssetType.Portrait, 4), m.EnumToId(OneBasedByte.Two));
            Assert.Equal(new AssetId(AssetType.Portrait, 5), m.EnumToId(OneBasedByte.Three));

            Assert.Throws<InvalidOperationException>(() => m.RegisterAssetType(typeof(int), AssetType.Automap));
            Assert.Throws<ArgumentNullException>(() => m.RegisterAssetType((Type)null, AssetType.Monster));
        }

        [Fact]
        public void TestFirstRegistrationOffsetPreserved()
        {
            var m = AssetMapping.Global.Clear();
            m.RegisterAssetType(typeof(OneBasedByte), AssetType.Map);
            m.RegisterAssetType(typeof(ZeroBasedByte), AssetType.Map);
            Assert.Equal(new AssetId(AssetType.Map, 1), AssetId.From(OneBasedByte.One));
            Assert.Equal(new AssetId(AssetType.Map, 2), AssetId.From(OneBasedByte.Two));
            Assert.Equal(new AssetId(AssetType.Map, 3), AssetId.From(OneBasedByte.Three));
            Assert.Equal(new AssetId(AssetType.Map, 4), AssetId.From(ZeroBasedByte.Zero));
            Assert.Equal(new AssetId(AssetType.Map, 5), AssetId.From(ZeroBasedByte.One));
            Assert.Equal(new AssetId(AssetType.Map, 6), AssetId.From(ZeroBasedByte.Two));
        }

        [Fact]
        public void TestDoubleRegistration()
        {
            var m = new AssetMapping().RegisterAssetType(typeof(ZeroBasedByte), AssetType.Portrait);
            Assert.Throws<InvalidOperationException>(() => m.RegisterAssetType(typeof(ZeroBasedByte), AssetType.Portrait));
            Assert.Throws<InvalidOperationException>(() => m.RegisterAssetType(typeof(ZeroBasedByte), AssetType.Automap));
        }

        [Fact]
        public void TestEnumToId()
        {
            var m = AssetMapping.Global.Clear().RegisterAssetType(typeof(GapByteOne), AssetType.Item);
            Assert.Equal(new AssetId(AssetType.Item, 1), m.EnumToId(GapByteOne.One));
            Assert.Equal(new AssetId(AssetType.Item, 2), m.EnumToId(GapByteOne.Two));
            Assert.Equal(new AssetId(AssetType.Item, 255), m.EnumToId(GapByteOne.Foo255));
            Assert.Equal(new AssetId(AssetType.Item, 200), m.EnumToId((GapByteOne)200));
            Assert.Throws<ArgumentOutOfRangeException>(() => m.EnumToId(ZeroBasedByte.Zero));

            Assert.Equal(new AssetId(AssetType.Item, 1), m.EnumToId(typeof(GapByteOne), 1));
            Assert.Equal(new AssetId(AssetType.Item, 2), m.EnumToId(typeof(GapByteOne), 2));
            Assert.Equal(new AssetId(AssetType.Item, 255), m.EnumToId(typeof(GapByteOne), 255));
            Assert.Equal(new AssetId(AssetType.Item, 200), m.EnumToId(typeof(GapByteOne), 200));
            Assert.Throws<ArgumentOutOfRangeException>(() => m.EnumToId(typeof(ZeroBasedByte), 0));
        }

        [Fact]
        public void TestIdToEnum()
        {
            var m = AssetMapping.Global.Clear().RegisterAssetType(typeof(GapByteOne), AssetType.Item);
            Assert.Equal((typeof(GapByteOne), (int)GapByteOne.One), m.IdToEnum(new AssetId(AssetType.Item, 1)));
            Assert.Equal((typeof(GapByteOne), (int)GapByteOne.Two), m.IdToEnum(new AssetId(AssetType.Item, 2)));
            Assert.Equal((typeof(GapByteOne), (int)GapByteOne.Foo255), m.IdToEnum(new AssetId(AssetType.Item, 255)));
            Assert.Equal((typeof(GapByteOne), 200), m.IdToEnum(new AssetId(AssetType.Item, 200)));
            var invalidId = new AssetId(AssetType.Item, 300);
            Assert.Equal((null, invalidId.ToInt32()), m.IdToEnum(invalidId));
        }

        // Test To/FromDisk on AssetId

        [Fact]
        public void RoundtripTest()
        {
            var m = AssetMapping.Global.Clear();
            m
                .RegisterAssetType(typeof(ZeroBasedByte), AssetType.Portrait)
                .RegisterAssetType(typeof(OneBasedByte), AssetType.Portrait)
                .RegisterAssetType(typeof(GapByteZero), AssetType.Map)
                .RegisterAssetType(typeof(ZeroBasedShort), AssetType.Map)
                ;

            Assert.Collection(m.EnumeratAssetsOfType(AssetType.Portrait),
                x => Assert.Equal("ZeroBasedByte.Zero", x.ToString()),
                x => Assert.Equal("ZeroBasedByte.One", x.ToString()),
                x => Assert.Equal("ZeroBasedByte.Two", x.ToString()),
                x => Assert.Equal("OneBasedByte.One", x.ToString()),
                x => Assert.Equal("OneBasedByte.Two", x.ToString()),
                x => Assert.Equal("OneBasedByte.Three", x.ToString())
                );

            Assert.Collection(m.EnumeratAssetsOfType(AssetType.Map),
                x => Assert.Equal("GapByteZero.Zero", x.ToString()),
                x => Assert.Equal("GapByteZero.One", x.ToString()),
                x => Assert.Equal("GapByteZero.Foo255", x.ToString()),
                x => Assert.Equal("ZeroBasedShort.Zero", x.ToString()),
                x => Assert.Equal("ZeroBasedShort.One", x.ToString()),
                x => Assert.Equal("ZeroBasedShort.Two", x.ToString())
                );

            var json = m.Serialize(new JsonSerializerSettings { Formatting = Formatting.None });
            const string expectedJson = "{" + 
                @"""UAlbion.Formats.Tests.AssetMappingTests+ZeroBasedByte, UAlbion.Formats.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"":{""AssetType"":""Portrait"",""EnumMin"":0,""EnumMax"":2,""Offset"":0,""Ranges"":[{""From"":0,""To"":2}]}," + 
                @"""UAlbion.Formats.Tests.AssetMappingTests+OneBasedByte, UAlbion.Formats.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"":{""AssetType"":""Portrait"",""EnumMin"":1,""EnumMax"":3,""Offset"":2,""Ranges"":[{""From"":3,""To"":5}]}," +
                @"""UAlbion.Formats.Tests.AssetMappingTests+GapByteZero, UAlbion.Formats.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"":{""AssetType"":""Map"",""EnumMin"":0,""EnumMax"":255,""Offset"":0,""Ranges"":[{""From"":0,""To"":1},{""From"":255,""To"":255}]}," +
                @"""UAlbion.Formats.Tests.AssetMappingTests+ZeroBasedShort, UAlbion.Formats.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"":{""AssetType"":""Map"",""EnumMin"":0,""EnumMax"":2,""Offset"":256,""Ranges"":[{""From"":256,""To"":258}]}"
                + "}";
            Assert.Equal(expectedJson, json);

            var roundTripped = AssetMapping.Deserialize(json);
            Assert.Collection(roundTripped.EnumeratAssetsOfType(AssetType.Portrait),
                x => Assert.Equal("ZeroBasedByte.Zero", x.ToString()),
                x => Assert.Equal("ZeroBasedByte.One", x.ToString()),
                x => Assert.Equal("ZeroBasedByte.Two", x.ToString()),
                x => Assert.Equal("OneBasedByte.One", x.ToString()),
                x => Assert.Equal("OneBasedByte.Two", x.ToString()),
                x => Assert.Equal("OneBasedByte.Three", x.ToString())
                );

            Assert.Collection(roundTripped.EnumeratAssetsOfType(AssetType.Map),
                x => Assert.Equal("GapByteZero.Zero", x.ToString()),
                x => Assert.Equal("GapByteZero.One", x.ToString()),
                x => Assert.Equal("GapByteZero.Foo255", x.ToString()),
                x => Assert.Equal("ZeroBasedShort.Zero", x.ToString()),
                x => Assert.Equal("ZeroBasedShort.One", x.ToString()),
                x => Assert.Equal("ZeroBasedShort.Two", x.ToString())
                );
        }
    }
}
