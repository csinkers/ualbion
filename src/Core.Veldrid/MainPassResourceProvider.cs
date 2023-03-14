using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public class MainPassResourceProvider : Component, IResourceProvider, IDisposable
{
    readonly SingleBuffer<CameraUniform> _camera;
    readonly MainPassSet _passSet;

    public IResourceSetHolder ResourceSet => _passSet;

    public MainPassResourceProvider(IFramebufferHolder target)
    {
        _camera = new SingleBuffer<CameraUniform>(BufferUsage.UniformBuffer | BufferUsage.Dynamic, "B_Camera");
        _passSet = new MainPassSet
        {
            Name = "RS_MainPass",
            Camera = _camera,
        };

        AttachChild(_camera);
        AttachChild(_passSet);
        On<PrepareFrameEvent>(_ => UpdatePerFrameResources(target));
    }

    void UpdatePerFrameResources(IFramebufferHolder target)
    {
        var camera = Resolve<ICamera>();
        camera.Viewport = new Vector2(target.Width, target.Height);

        _camera.Data = new CameraUniform
        {
            WorldSpacePosition = camera.Position,
            CameraDirection = new Vector2(camera.Pitch, camera.Yaw),
            Resolution =  new Vector2(target.Width, target.Height),
            Projection = camera.ProjectionMatrix,
            View = camera.ViewMatrix,
        };
    }

    public void Dispose()
    {
        _passSet?.Dispose();
        _camera?.Dispose();
        GC.SuppressFinalize(this);
    }
}