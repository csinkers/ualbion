using System;
using System.Numerics;
using Xunit;

namespace UAlbion.Formats.Tests
{
    public class CircumcircleTests
    {
        void Verify(Geometry.Triangle t, Vector2 c, float r2)
        {
            var az = (t.A - c).LengthSquared();
            var bz = (t.B - c).LengthSquared();
            var cz = (t.C - c).LengthSquared();
            Assert.True(Math.Abs(az - bz) < 0.001); // radials from centre to each point should have equal length
            Assert.True(Math.Abs(az - cz) < 0.001);
            Assert.True(Math.Abs(az - r2) < 0.001); // radial lengths should match returned radius
        }

        [Fact]
        public void EquilateralTest()
        {
            var a = new Vector2(-1, 0);
            var b = new Vector2(1, 0);
            var c = new Vector2(0, (float)Math.Sqrt(3));
            Assert.True(Math.Abs((a - b).Length() - 2) < float.Epsilon);
            Assert.True(Math.Abs((b - c).Length() - 2) < float.Epsilon);
            Assert.True(Math.Abs((c - a).Length() - 2) < float.Epsilon);

            var triangle = new Geometry.Triangle(a, b, c);
            var (z, r2) = triangle.Circumcircle();
            Verify(triangle, z, r2);
        }

        [Fact]
        public void IsoscelesTest()
        {
            var a = new Vector2(-0.5f, 0);
            var b = new Vector2(0.5f, 0);
            var c = new Vector2(0, 2);
            Assert.True(Math.Abs((a - c).Length() - (b - c).Length()) < float.Epsilon);

            var triangle = new Geometry.Triangle(a, b, c);
            var (z, r2) = triangle.Circumcircle();
            Verify(triangle, z, r2);
        }

        [Fact]
        public void ScaleneTest()
        {
            var a = new Vector2(12, 0);
            var b = new Vector2(-6, -4);
            var c = new Vector2(7.2f, 8.1f);

            var triangle = new Geometry.Triangle(a, b, c);
            var (z, r2) = triangle.Circumcircle();
            Verify(triangle, z, r2);
        }

        [Fact]
        public void ColinearTest()
        {
            var a = new Vector2(-1, 0);
            var b = new Vector2(0, 0);
            var c = new Vector2(1, 0);

            var triangle = new Geometry.Triangle(a, b, c);
            var (z, r2) = triangle.Circumcircle();
            Assert.Equal(float.NaN, z.X);
            Assert.Equal(float.PositiveInfinity, z.Y);
            Assert.Equal(float.NaN, r2);
        }

        [Fact]
        public void ColocatedTest()
        {
            var a = new Vector2(0, 0);
            var b = new Vector2(0, 0);
            var c = new Vector2(0, 0);

            var triangle = new Geometry.Triangle(a, b, c);
            var (z, r2) = triangle.Circumcircle();
            Assert.Equal(float.NaN, z.X);
            Assert.Equal(float.NaN, z.Y);
            Assert.Equal(float.NaN, r2);
        }
    }
}