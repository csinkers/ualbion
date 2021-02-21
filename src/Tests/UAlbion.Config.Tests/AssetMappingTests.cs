using System;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace UAlbion.Config.Tests
{
    public class AssetMappingTests : Component
    {
        readonly ITestOutputHelper _output;
        enum ZeroBasedByte : byte { Zero = 0, One, Two }
        enum OneBasedByte : byte { One = 1, Two, Three }
        enum ZeroBasedShort : ushort { Zero = 0, One, Two }
        enum GapByteZero : byte { Zero = 0, One, Foo255 = 255 }
        enum GapByteOne : byte { One = 1, Two, Foo255 = 255 }

        public AssetMappingTests(ITestOutputHelper output)
        {
            _output = output;
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

            Assert.Equal(new AssetId(AssetType.Portrait, 0), m.EnumToId(typeof(ZeroBasedByte), 0));
            Assert.Equal(new AssetId(AssetType.Portrait, 1), m.EnumToId(typeof(ZeroBasedByte), 1));
            Assert.Equal(new AssetId(AssetType.Portrait, 2), m.EnumToId(typeof(ZeroBasedByte), 2));
            Assert.Throws<ArgumentOutOfRangeException>(() => m.EnumToId(typeof(ZeroBasedByte), 3));

            m.RegisterAssetType(typeof(OneBasedByte), AssetType.Portrait);
            Assert.Equal(new AssetId(AssetType.Portrait, 3), m.EnumToId(OneBasedByte.One));
            Assert.Equal(new AssetId(AssetType.Portrait, 4), m.EnumToId(OneBasedByte.Two));
            Assert.Equal(new AssetId(AssetType.Portrait, 5), m.EnumToId(OneBasedByte.Three));

            Assert.Equal(AssetId.None, m.EnumToId(typeof(OneBasedByte), 0)); // Hmm... should this throw?
            Assert.Equal(new AssetId(AssetType.Portrait, 3), m.EnumToId(typeof(OneBasedByte), 1));
            Assert.Equal(new AssetId(AssetType.Portrait, 4), m.EnumToId(typeof(OneBasedByte), 2));
            Assert.Equal(new AssetId(AssetType.Portrait, 5), m.EnumToId(typeof(OneBasedByte), 3));

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
            // Used to throw, but now we forgive it to prevent issues when a mod overrides some assets using a dependency's ids.
            m.RegisterAssetType(typeof(ZeroBasedByte), AssetType.Portrait); 
            m.RegisterAssetType(typeof(ZeroBasedByte), AssetType.Automap);
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

            Assert.Collection(m.EnumerateAssetsOfType(AssetType.Portrait),
                x => Assert.Equal("ZeroBasedByte.Zero", x.ToString()),
                x => Assert.Equal("ZeroBasedByte.One", x.ToString()),
                x => Assert.Equal("ZeroBasedByte.Two", x.ToString()),
                x => Assert.Equal("OneBasedByte.One", x.ToString()),
                x => Assert.Equal("OneBasedByte.Two", x.ToString()),
                x => Assert.Equal("OneBasedByte.Three", x.ToString())
            );

            Assert.Collection(m.EnumerateAssetsOfType(AssetType.Map),
                x => Assert.Equal("GapByteZero.Zero", x.ToString()),
                x => Assert.Equal("GapByteZero.One", x.ToString()),
                x => Assert.Equal("GapByteZero.Foo255", x.ToString()),
                x => Assert.Equal("ZeroBasedShort.Zero", x.ToString()),
                x => Assert.Equal("ZeroBasedShort.One", x.ToString()),
                x => Assert.Equal("ZeroBasedShort.Two", x.ToString())
            );

            var json = m.Serialize(new JsonSerializerSettings { Formatting = Formatting.None });
            const string expectedJson = "{" + 
                                        @"""UAlbion.Config.Tests.AssetMappingTests+ZeroBasedByte, UAlbion.Config.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"":{""AssetType"":""Portrait"",""EnumMin"":0,""EnumMax"":2,""Offset"":0,""Ranges"":[{""From"":0,""To"":2}]}," + 
                                        @"""UAlbion.Config.Tests.AssetMappingTests+OneBasedByte, UAlbion.Config.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"":{""AssetType"":""Portrait"",""EnumMin"":1,""EnumMax"":3,""Offset"":2,""Ranges"":[{""From"":3,""To"":5}]}," +
                                        @"""UAlbion.Config.Tests.AssetMappingTests+GapByteZero, UAlbion.Config.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"":{""AssetType"":""Map"",""EnumMin"":0,""EnumMax"":255,""Offset"":0,""Ranges"":[{""From"":0,""To"":1},{""From"":255,""To"":255}]}," +
                                        @"""UAlbion.Config.Tests.AssetMappingTests+ZeroBasedShort, UAlbion.Config.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"":{""AssetType"":""Map"",""EnumMin"":0,""EnumMax"":2,""Offset"":256,""Ranges"":[{""From"":256,""To"":258}]}"
                                        + "}";
            Assert.Equal(expectedJson, json);

            var roundTripped = AssetMapping.Deserialize(json);
            Assert.Collection(roundTripped.EnumerateAssetsOfType(AssetType.Portrait),
                x => Assert.Equal("ZeroBasedByte.Zero", x.ToString()),
                x => Assert.Equal("ZeroBasedByte.One", x.ToString()),
                x => Assert.Equal("ZeroBasedByte.Two", x.ToString()),
                x => Assert.Equal("OneBasedByte.One", x.ToString()),
                x => Assert.Equal("OneBasedByte.Two", x.ToString()),
                x => Assert.Equal("OneBasedByte.Three", x.ToString())
            );

            Assert.Collection(roundTripped.EnumerateAssetsOfType(AssetType.Map),
                x => Assert.Equal("GapByteZero.Zero", x.ToString()),
                x => Assert.Equal("GapByteZero.One", x.ToString()),
                x => Assert.Equal("GapByteZero.Foo255", x.ToString()),
                x => Assert.Equal("ZeroBasedShort.Zero", x.ToString()),
                x => Assert.Equal("ZeroBasedShort.One", x.ToString()),
                x => Assert.Equal("ZeroBasedShort.Two", x.ToString())
            );
        }

        [Fact]
        public void ParseTextualTest()
        {
            var m = AssetMapping.Global.Clear();
            Assert.Equal(AssetId.None, m.Parse("", null));
            m.RegisterAssetType(typeof(ZeroBasedByte), AssetType.Portrait);

            Assert.Equal(AssetId.None, m.Parse("", null));
            Assert.Equal(AssetId.From(ZeroBasedByte.Zero), m.Parse("Zero", null));
            Assert.Equal(AssetId.From(ZeroBasedByte.One), m.Parse("One", null));
            Assert.Equal(AssetId.From(ZeroBasedByte.Zero), m.Parse("Portrait.Zero", null));
            Assert.Equal(AssetId.From(ZeroBasedByte.One), m.Parse("Portrait.One", null));
            Assert.Throws<FormatException>(() => m.Parse("Npc.Zero", null));
            Assert.Throws<FormatException>(() => m.Parse("Npc.One", null));
        }

        [Fact]
        public void ParseNumericTest()
        {
            var m = AssetMapping.Global.Clear();
            m.RegisterAssetType(typeof(ZeroBasedByte), AssetType.Portrait);
            m.RegisterAssetType(typeof(OneBasedByte), AssetType.Npc);

            Assert.Equal(new AssetId(AssetType.Unknown, 0), m.Parse("somethinginvalid", null));
            Assert.Equal(new AssetId(AssetType.Unknown, 0), m.Parse("Portrait.nonsense", null));
            Assert.Throws<FormatException>(() => m.Parse("0", null));
            Assert.Equal(AssetId.From(ZeroBasedByte.Zero), m.Parse("0", new[] { AssetType.Portrait }));
            Assert.Equal(AssetId.From(ZeroBasedByte.One), m.Parse("1", new[] { AssetType.Portrait }));
            Assert.Equal(AssetId.From(OneBasedByte.One), m.Parse("1", new[] { AssetType.Npc }));
            // Assert.Equal(AssetId.None, m.Parse("0", new[] { AssetType.Npc })); // TODO: Decide if this should be parse to None, an unmapped Npc.0 id, or throw.

            Assert.Equal(AssetId.From(ZeroBasedByte.Zero), m.Parse("Portrait.0", null));
            Assert.Equal(AssetId.From(ZeroBasedByte.One), m.Parse("Portrait.1", null));
            Assert.Equal(AssetId.From(OneBasedByte.One), m.Parse("Npc.1", null));
            Assert.Equal(AssetId.From(OneBasedByte.Two), m.Parse("Npc.2", null));

            m.RegisterAssetType(typeof(ZeroBasedShort), AssetType.Portrait);
            Assert.Equal(AssetId.From(ZeroBasedByte.Zero), m.Parse("0", new[] { AssetType.Portrait }));
            Assert.Equal(AssetId.From(ZeroBasedByte.One), m.Parse("1", new[] { AssetType.Portrait }));
            Assert.Equal(AssetId.From(ZeroBasedByte.Two), m.Parse("2", new[] { AssetType.Portrait }));
            Assert.Equal(AssetId.From(ZeroBasedShort.Zero), m.Parse("3", new[] { AssetType.Portrait }));
            Assert.Equal(AssetId.From(ZeroBasedShort.One), m.Parse("4", new[] { AssetType.Portrait }));
            Assert.Equal(AssetId.From(ZeroBasedShort.Two), m.Parse("5", new[] { AssetType.Portrait }));
        }

        [Fact]
        public void AmbiguousParseTest()
        {
            var m = AssetMapping.Global.Clear();
            m.RegisterAssetType(typeof(ZeroBasedByte), AssetType.Portrait);
            m.RegisterAssetType(typeof(OneBasedByte), AssetType.Npc);
            _output.WriteLine(m.Serialize());
            Assert.Equal(AssetId.From(ZeroBasedByte.Zero), m.Parse("Zero", null));
            Assert.Throws<FormatException>(() => m.Parse("One", null)); // Ambiguous
            Assert.Equal(AssetId.From(ZeroBasedByte.One), m.Parse("One", new[] {AssetType.Portrait}));
            Assert.Equal(AssetId.From(OneBasedByte.One), m.Parse("One", new[] {AssetType.Npc}));
            Assert.Throws<FormatException>(() =>
            m.Parse("One", new[] {AssetType.Portrait, AssetType.Npc})); // Ambiguous
        }

        [Fact]
        public void ParseWithTypePrefixTest()
        {
            var m = AssetMapping.Global.Clear();
            m.RegisterAssetType(typeof(ZeroBasedByte), AssetType.Portrait);
            m.RegisterAssetType(typeof(OneBasedByte), AssetType.Npc);
            _output.WriteLine(m.Serialize());
            Assert.Equal(AssetId.From(ZeroBasedByte.One), m.Parse("Portrait.One", null));
            Assert.Equal(AssetId.From(OneBasedByte.One), m.Parse("Npc.One", null));
        }

        [Fact]
        public void ParseUnknownTest()
        {
            var m = AssetMapping.Global.Clear();
            Assert.Equal(new AssetId(AssetType.Unknown, 1), m.Parse("Unknown.1", null));
            Assert.Equal(new AssetId(AssetType.Unknown, 2), m.Parse("Unknown.2", null));
            Assert.Equal(new AssetId(AssetType.Unknown, 0), m.Parse("Unknown.0", null));
        }

        [Fact]
        public void IdToEnumStringTest()
        {
            var m = AssetMapping.Global.Clear();
            m.RegisterAssetType(typeof(ZeroBasedByte), AssetType.Portrait);
            m.RegisterAssetType(typeof(OneBasedByte), AssetType.Npc);

            var zbb = "UAlbion.Config.Tests.AssetMappingTests+ZeroBasedByte, UAlbion.Config.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            var obb = "UAlbion.Config.Tests.AssetMappingTests+OneBasedByte, UAlbion.Config.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            Assert.Equal((zbb, 0), m.IdToEnumString(AssetId.From(ZeroBasedByte.Zero)));
            Assert.Equal((zbb, 1), m.IdToEnumString(AssetId.From(ZeroBasedByte.One)));
            Assert.Equal((zbb, 2), m.IdToEnumString(AssetId.From(ZeroBasedByte.Two)));
            Assert.Equal((obb, 1), m.IdToEnumString(AssetId.From(OneBasedByte.One)));
            Assert.Equal((obb, 2), m.IdToEnumString(AssetId.From(OneBasedByte.Two)));
            Assert.Equal((obb, 3), m.IdToEnumString(AssetId.From(OneBasedByte.Three)));
        }

        [Fact]
        public void MergeTest()
        {
            var m1 = new AssetMapping();
            var m2 = new AssetMapping();
            m1.RegisterAssetType(typeof(ZeroBasedByte), AssetType.Portrait);
            m2.RegisterAssetType(typeof(OneBasedByte), AssetType.Portrait);

            Assert.Equal(new AssetId(AssetType.Portrait, 1), m1.EnumToId(ZeroBasedByte.One));
            Assert.Throws<ArgumentOutOfRangeException>(() => m1.EnumToId(OneBasedByte.One));
            Assert.Throws<ArgumentOutOfRangeException>(() => m2.EnumToId(ZeroBasedByte.One));
            Assert.Equal(new AssetId(AssetType.Portrait, 1), m2.EnumToId(OneBasedByte.One));

            Assert.Throws<ArgumentNullException>(() => m2.MergeFrom(null));
            m2.MergeFrom(m1);
            Assert.Equal(new AssetId(AssetType.Portrait, 1), m1.EnumToId(ZeroBasedByte.One));
            Assert.Throws<ArgumentOutOfRangeException>(() => m1.EnumToId(OneBasedByte.One));
            Assert.Equal(new AssetId(AssetType.Portrait, 5), m2.EnumToId(ZeroBasedByte.One));
            Assert.Equal(new AssetId(AssetType.Portrait, 1), m2.EnumToId(OneBasedByte.One));

            m2.RegisterAssetType(typeof(ZeroBasedShort), AssetType.Portrait);
            Assert.Throws<ArgumentOutOfRangeException>(() => m1.EnumToId(ZeroBasedShort.Zero));
            Assert.Equal(new AssetId(AssetType.Portrait, 7), m2.EnumToId(ZeroBasedShort.Zero));

            m1.MergeFrom(m2);
            Assert.Equal(new AssetId(AssetType.Portrait, 1), m1.EnumToId(ZeroBasedByte.One));
            Assert.Equal(new AssetId(AssetType.Portrait, 3), m1.EnumToId(OneBasedByte.One));
            Assert.Equal(new AssetId(AssetType.Portrait, 6), m1.EnumToId(ZeroBasedShort.Zero));

            Assert.Collection(
                new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }.Select(x => m1.IdToEnum(new AssetId(AssetType.Portrait, x))),
                x => { Assert.Equal(typeof(ZeroBasedByte), x.Item1); Assert.Equal(0, x.Item2); },
                x => { Assert.Equal(typeof(ZeroBasedByte), x.Item1); Assert.Equal(1, x.Item2); },
                x => { Assert.Equal(typeof(ZeroBasedByte), x.Item1); Assert.Equal(2, x.Item2); },
                x => { Assert.Equal(typeof(OneBasedByte), x.Item1); Assert.Equal(1, x.Item2); },
                x => { Assert.Equal(typeof(OneBasedByte), x.Item1); Assert.Equal(2, x.Item2); },
                x => { Assert.Equal(typeof(OneBasedByte), x.Item1); Assert.Equal(3, x.Item2); },
                x => { Assert.Equal(typeof(ZeroBasedShort), x.Item1); Assert.Equal(0, x.Item2); },
                x => { Assert.Equal(typeof(ZeroBasedShort), x.Item1); Assert.Equal(1, x.Item2); },
                x => { Assert.Equal(typeof(ZeroBasedShort), x.Item1); Assert.Equal(2, x.Item2); }
            );

            Assert.Collection(
                new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }.Select(x => m2.IdToEnum(new AssetId(AssetType.Portrait, x))),
                x => { Assert.Equal(typeof(OneBasedByte), x.Item1); Assert.Equal(0, x.Item2); },
                x => { Assert.Equal(typeof(OneBasedByte), x.Item1); Assert.Equal(1, x.Item2); },
                x => { Assert.Equal(typeof(OneBasedByte), x.Item1); Assert.Equal(2, x.Item2); },
                x => { Assert.Equal(typeof(OneBasedByte), x.Item1); Assert.Equal(3, x.Item2); },
                x => { Assert.Equal(typeof(ZeroBasedByte), x.Item1); Assert.Equal(0, x.Item2); },
                x => { Assert.Equal(typeof(ZeroBasedByte), x.Item1); Assert.Equal(1, x.Item2); },
                x => { Assert.Equal(typeof(ZeroBasedByte), x.Item1); Assert.Equal(2, x.Item2); },
                x => { Assert.Equal(typeof(ZeroBasedShort), x.Item1); Assert.Equal(0, x.Item2); },
                x => { Assert.Equal(typeof(ZeroBasedShort), x.Item1); Assert.Equal(1, x.Item2); },
                x => { Assert.Equal(typeof(ZeroBasedShort), x.Item1); Assert.Equal(2, x.Item2); }
            );
        }
    }
}