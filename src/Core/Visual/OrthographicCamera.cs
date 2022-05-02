using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;

namespace UAlbion.Core.Visual;

public class OrthographicCamera : ServiceComponent<ICamera>, ICamera
{
    readonly bool _yAxisIncreasesDownTheScreen;
    Vector3 _position = new(0, 0, 0);
    Vector3 _lookDirection = new(0, 0, -1f);
    Vector2 _viewport = Vector2.One;
    Matrix4x4 _viewMatrix;       
    Matrix4x4 _projectionMatrix; 
    float _magnification = 1.0f; // TODO: Ensure this defaults to something sensible, and at some point lock it to a value that fits the gameplay and map design.
    float _yaw;
    float _pitch;
    float _farDistance = 1;
    bool _dirty = true;

    public int Version { get; private set; }
    public Matrix4x4 ViewMatrix { get { Recalculate(); return _viewMatrix; } }
    public Matrix4x4 ProjectionMatrix { get { Recalculate(); return _projectionMatrix; } }
    void Recalculate()
    {
        if (!_dirty)
            return;
        _viewMatrix = CalculateView();
        _projectionMatrix = CalculateProjection();
        Version++;
        _dirty = false;
    }

    public Vector3 Position { get => _position; set { _position = value; _dirty = true; } }
    public float Magnification { get => _magnification; private set { _magnification = value; _dirty = true; } }
    public float FarDistance { get => _farDistance; private set { _farDistance = value; _dirty = true; } } 
    public float Yaw { get => _yaw; private set { _yaw = value; _dirty = true; } } // Radians
    public float Pitch { get => _pitch; private set { _pitch = Math.Clamp(value, (float)-Math.PI / 2, (float)Math.PI / 2); _dirty = true; } } // Radians
    public float AspectRatio => _viewport.X / _viewport.Y;
    public Vector3 LookDirection => _lookDirection;
    public float FieldOfView => 1f;
    public float NearDistance => 0;

    public Vector2 Viewport
    {
        get => _viewport;
        set
        {
            if (_viewport == value)
                return;
            _viewport = value;
            _dirty = true;
        }
    }

    public OrthographicCamera(bool yAxisIncreasesDownTheScreen = true)
    {
        _yAxisIncreasesDownTheScreen = yAxisIncreasesDownTheScreen;
        OnAsync<ScreenCoordinateSelectEvent, Selection>(TransformSelect);
        On<BackendChangedEvent>(_ => _dirty = true);
        On<CameraPositionEvent>(e => Position = new Vector3(e.X, e.Y, e.Z));
        On<CameraPlanesEvent>(e => FarDistance = e.Far);
        On<CameraDirectionEvent>(e => { Yaw = ApiUtil.DegToRad(e.Yaw); Pitch = ApiUtil.DegToRad(e.Pitch); });
        On<CameraMagnificationEvent>(e => { Magnification = e.Magnification; });

        On<MagnifyEvent>(e =>
        {
            if (_magnification < 1.0f && e.Delta > 0)
                _magnification = 0.0f;

            _magnification += e.Delta;

            if (_magnification < 0.5f)
                _magnification = 0.5f;
            _dirty = true;
            Raise(new CameraMagnificationEvent(_magnification));
        });
    }

    protected override void Subscribed()
    {
        base.Subscribed();
        _dirty = true;
    }

    bool TransformSelect(ScreenCoordinateSelectEvent e, Action<Selection> continuation)
    {
        var normalisedScreenPosition = new Vector3(2 * e.Position.X / _viewport.X - 1.0f, -2 * e.Position.Y / _viewport.Y + 1.0f, 0.0f);
        var rayOrigin = UnprojectNormToWorld(normalisedScreenPosition + Vector3.UnitZ);
        var rayDirection = UnprojectNormToWorld(normalisedScreenPosition) - rayOrigin;
        RaiseAsync(new WorldCoordinateSelectEvent(rayOrigin, rayDirection, e.Debug), continuation);
        return true;
    }

    Matrix4x4 CalculateProjection()
    {
        var projectionMatrix = Matrix4x4.Identity;
        projectionMatrix.M11 = 2.0f * _magnification / _viewport.X;
        projectionMatrix.M22 = 2.0f * _magnification / _viewport.Y;
        projectionMatrix.M33 = 1 / FarDistance;

        var e = TryResolve<IEngine>();
        if (e != null && _yAxisIncreasesDownTheScreen != e.IsClipSpaceYInverted)
            projectionMatrix.M22 = -projectionMatrix.M22;
        return projectionMatrix;
    }

    Matrix4x4 CalculateView()
    {
        Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
        _lookDirection = Vector3.Transform(-Vector3.UnitZ, lookRotation);
        return Matrix4x4.CreateTranslation(-_position) * Matrix4x4.CreateFromQuaternion(lookRotation);
    }

    public Vector3 ProjectWorldToNorm(Vector3 worldPosition) => Vector3.Transform(worldPosition, ViewMatrix * ProjectionMatrix) - Vector3.UnitZ;
    public Vector3 UnprojectNormToWorld(Vector3 normPosition)
    {
        var totalMatrix = ViewMatrix * ProjectionMatrix;
        var inverse = totalMatrix.Inverse();
        return Vector3.Transform(normPosition + Vector3.UnitZ, inverse);
    }
}