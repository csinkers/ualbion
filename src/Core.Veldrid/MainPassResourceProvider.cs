using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public sealed class MainPassResourceProvider : Component, IResourceProvider, IDisposable
{
    readonly ICameraProvider _cameraProvider;
    readonly SingleBuffer<CameraUniform> _cameraUniform;
    readonly MainPassSet _passSet;

    public IResourceSetHolder ResourceSet => _passSet;

    public MainPassResourceProvider(IFramebufferHolder target, ICameraProvider cameraProvider)
    {
        _cameraProvider = cameraProvider ?? throw new ArgumentNullException(nameof(cameraProvider));
        _cameraUniform = new SingleBuffer<CameraUniform>(BufferUsage.UniformBuffer | BufferUsage.Dynamic, "B_Camera");
        _passSet = new MainPassSet
        {
            Name = "RS_MainPass",
            Camera = _cameraUniform,
        };

        AttachChild(_cameraUniform);
        AttachChild(_passSet);
        On<PrepareFrameEvent>(_ => UpdatePerFrameResources(target));
    }

    void UpdatePerFrameResources(IFramebufferHolder target)
    {
        var camera = _cameraProvider.Camera;
        camera.Viewport = new Vector2(target.Width, target.Height);

        _cameraUniform.Data = new CameraUniform
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
        _cameraUniform?.Dispose();
    }
}