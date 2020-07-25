using System;
using Xunit;

namespace UAlbion.Api.Tests
{
    public class ApiUtilTests
    {
        [Fact]
        public void LerpTest()
        {
            Assert.Equal(10, ApiUtil.Lerp(10, 20, 0));
            Assert.Equal(15, ApiUtil.Lerp(10, 20, 0.5f));
            Assert.Equal(20, ApiUtil.Lerp(10, 20, 1));

            Assert.Equal(1, ApiUtil.Lerp(1, -1, 0));
            Assert.Equal(0, ApiUtil.Lerp(1, -1, 0.5f));
            Assert.Equal(-1, ApiUtil.Lerp(1, -1, 1));
        }

        [Fact]
        public void DegToRadTest()
        {
            Assert.Equal(0, ApiUtil.DegToRad(0));
            Assert.True(Math.Abs(Math.PI - ApiUtil.DegToRad(180)) <= 1e-7);
        }

        [Fact]
        public void RadToDegTest()
        {
            Assert.Equal(0, ApiUtil.RadToDeg(0));
            Assert.True(Math.Abs(180 - ApiUtil.RadToDeg((float) Math.PI)) <= float.Epsilon);
        }

        [Fact]
        public void LcmTest()
        {
            Assert.Equal(1, ApiUtil.Lcm(1, 1));
            Assert.Equal(4, ApiUtil.Lcm(1, 4));
            Assert.Equal(4, ApiUtil.Lcm(2, 4));
            Assert.Equal(6, ApiUtil.Lcm(2, 3));
        }

        [Fact]
        public void GcdTest()
        {
            Assert.Equal(1, ApiUtil.Gcd(7, 19));
            Assert.Equal(3, ApiUtil.Gcd(3, 9));
            Assert.Equal(3, ApiUtil.Gcd(6, 9));
            Assert.Equal(3, ApiUtil.Gcd(12, 9));
            Assert.Equal(9, ApiUtil.Gcd(81, 9));
        }

        [Fact]
        public void RotateImageTest()
        {
            byte[] image = {
                1,2,3,
                4,5,6,
                7,8,9
            };

            byte[] rotated = {
                1,4,7,
                2,5,8,
                3,6,9
            };

            var result = new byte[9];
            ApiUtil.RotateImage(3, 3, image, result);
            Assert.Equal(rotated, result);

            image = new byte[] {
                1, 2, 3, 4,
                5, 6, 7, 8,
                9,10,11,12
            };

            rotated = new byte[] {
                1, 5,  9,
                2, 6, 10,
                3, 7, 11,
                4, 8, 12
            };

            result = new byte[12];
            ApiUtil.RotateImage(4, 3, image, result);
            Assert.Equal(rotated, result);
        }
    }
}
