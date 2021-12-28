using System.Numerics;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using Xunit;

namespace UAlbion.Core.Tests;

public class PerspectiveCameraTests
{
    [Fact]
    public void TestPickingRay()
    {
        var ee = new EventExchange(new LogExchange());
        var wm = new WindowManager { Resolution = (1024, 1024) };
        var camera = new PerspectiveCamera(true)
        {
            Position = Vector3.Zero,
            Yaw = 0,
            Pitch = 0
        };
        ee.Attach(wm).Attach(camera);
        ee.Raise(new SetFieldOfViewEvent(90), this);

        var near = -Vector3.UnitZ * camera.NearDistance;

        var proj0 = camera.ProjectWorldToNorm(near);
        var projX = camera.ProjectWorldToNorm(near + camera.NearDistance * Vector3.UnitX);
        var projY = camera.ProjectWorldToNorm(near + camera.NearDistance * Vector3.UnitY);
        var projZ = camera.ProjectWorldToNorm(near - camera.NearDistance * Vector3.UnitZ);

        var z2 = Vector3.UnitZ * (1.0f + camera.NearDistance / (camera.FarDistance - camera.NearDistance)) / 2;
        Assert.Equal(Vector3.Zero, proj0);
        Assert.Equal(Vector3.UnitX, projX);
        Assert.Equal(Vector3.UnitY, projY);
        Assert.Equal(z2, projZ);

        var proj2 = camera.ProjectWorldToNorm(2*near);
        var proj2X = camera.ProjectWorldToNorm(2*(near + camera.NearDistance * Vector3.UnitX));
        var proj2Y = camera.ProjectWorldToNorm(2*(near + camera.NearDistance * Vector3.UnitY));
        var proj2Z = camera.ProjectWorldToNorm(2*(near - camera.NearDistance * Vector3.UnitZ));
        Assert.Equal(z2, proj2);
        Assert.Equal(z2+Vector3.UnitX, proj2X);
        Assert.Equal(z2+Vector3.UnitY, proj2Y);
        Assert.Equal(z2 * 1.5f, proj2Z);

        Assert.Equal(near, camera.UnprojectNormToWorld(proj0));
        Assert.Equal(near + camera.NearDistance * Vector3.UnitX, camera.UnprojectNormToWorld(projX));
        Assert.Equal(near + camera.NearDistance * Vector3.UnitY, camera.UnprojectNormToWorld(projY));
        Assert.Equal(near - camera.NearDistance * Vector3.UnitZ, camera.UnprojectNormToWorld(projZ));
    }
}