using System.Numerics;
using UAlbion.Core.Visual;
using UAlbion.TestCommon;
using Xunit;

namespace UAlbion.Core.Tests
{
    public class OrthographicCameraTests
    {
        [Fact]
        public void TestPickingRay()
        {
            var ee = new EventExchange(new LogExchange());
            var wm = new WindowManager {Window = new MockWindow(1024, 1024)};
            var camera = new OrthographicCamera { Position = Vector3.Zero };
            ee.Attach(wm).Attach(camera);

            var proj0 = camera.ProjectWorldToNorm(Vector3.Zero);
            var projX = camera.ProjectWorldToNorm(Vector3.UnitX);
            var projY = camera.ProjectWorldToNorm(Vector3.UnitY);
            var projZ = camera.ProjectWorldToNorm(Vector3.UnitZ);

            Assert.Equal(Vector3.Zero, camera.UnprojectNormToWorld(proj0));
            Assert.Equal(Vector3.UnitX, camera.UnprojectNormToWorld(projX));
            Assert.Equal(Vector3.UnitY, camera.UnprojectNormToWorld(projY));
            Assert.Equal(Vector3.UnitZ, camera.UnprojectNormToWorld(projZ));
        }
    }
}