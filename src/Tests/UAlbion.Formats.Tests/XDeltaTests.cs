using System.Linq;
using Xunit;

namespace UAlbion.Formats.Tests
{
    public class XDeltaTests
    {
        [Fact]
        public void IdenticalDiff()
        {
            var diffs = XDelta.Compare(
                Enumerable.Range(0, 256).Select(x => (byte)x).ToArray(),
                Enumerable.Range(0, 256).Select(x => (byte)x).ToArray()).ToArray();
            Assert.Collection(diffs,
                d =>
                {
                    Assert.True(d.IsCopy);
                    Assert.Equal(0, d.Offset);
                    Assert.Equal(256, d.Length);
                });
        }

        [Fact]
        public void ExtraByte()
        {
            var a = new byte[]
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
                17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32,
            };
            var b = new byte[]
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
                17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33
            };

            var diffs = XDelta.Compare(a, b).ToArray();
            Assert.Collection(diffs, 
                d =>
                {
                    Assert.True(d.IsCopy);
                    Assert.Equal(0, d.Offset);
                    Assert.Equal(32, d.Length);
                },
                d =>
                {
                    Assert.False(d.IsCopy);
                    Assert.Equal(33, d.Value);
                }
            );
        }

        [Fact]
        public void MissingByte()
        {
            var a = new byte[]
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
                17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32,
            };
            var b = new byte[]
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
                17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31
            };

            var diffs = XDelta.Compare(a, b).ToArray();
            Assert.Collection(diffs, 
                d =>
                {
                    Assert.True(d.IsCopy);
                    Assert.Equal(0, d.Offset);
                    Assert.Equal(31, d.Length);
                }
            );

            b = new byte[]
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
                17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 33, 34
            };

            diffs = XDelta.Compare(a, b).ToArray();
            Assert.Collection(diffs, 
                d =>
                {
                    Assert.True(d.IsCopy);
                    Assert.Equal(0, d.Offset);
                    Assert.Equal(31, d.Length);
                },
                d =>
                {
                    Assert.False(d.IsCopy);
                    Assert.Equal(33, d.Value);
                },
                d =>
                {
                    Assert.False(d.IsCopy);
                    Assert.Equal(34, d.Value);
                }
            );
        }
    }
}