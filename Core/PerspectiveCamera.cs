using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core.Events;

namespace UAlbion.Core
{
    public class PerspectiveCamera : Component, ICamera
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<PerspectiveCamera, BackendChangedEvent>((x, e) => x.UpdateBackend(e)),
            new Handler<PerspectiveCamera, BeginFrameEvent>((x, e) => x._movementDirection = Vector3.Zero),
            new Handler<PerspectiveCamera, EngineCameraMoveEvent>((x, e) => x._movementDirection += new Vector3(e.X, 0, e.Y)),
            new Handler<PerspectiveCamera, EngineCameraRotateEvent>((x, e) => { x.Yaw += e.Yaw; x.Pitch += e.Pitch; }),
            new Handler<PerspectiveCamera, EngineUpdateEvent>((x, e) =>
            {
                if (x._movementDirection == Vector3.Zero)
                    return;

                Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(x.Yaw, x.Pitch, 0f);
                x._movementDirection = Vector3.Transform(Vector3.Normalize(x._movementDirection), lookRotation);
                x._position += x._movementDirection * e.DeltaSeconds;
                x.UpdateViewMatrix();
            }),
            new Handler<PerspectiveCamera, WindowResizedEvent> ((x, e) => x.WindowResized(e.Width, e.Height)),
        };

        Matrix4x4 _viewMatrix;
        Matrix4x4 _projectionMatrix;

        Vector3 _position = new Vector3(0, 48, 0);
        Vector3 _lookDirection = new Vector3(0, -.3f, -1f);
        Vector3 _movementDirection;

        float _yaw;
        float _pitch;
        bool _useReverseDepth;
        float _windowWidth;
        float _windowHeight;

        bool _isClipSpaceYInverted;

        public Matrix4x4 ViewMatrix => _viewMatrix;
        public Matrix4x4 ProjectionMatrix => _projectionMatrix;
        public Vector3 Position { get => _position; set { _position = value; UpdateViewMatrix(); } }
        public Vector3 LookDirection => _lookDirection;
        public float FieldOfView => 1f;
        public float NearDistance => 1f;
        public float FarDistance => 1000f;

        public PerspectiveCamera() : base(Handlers)
        {
            _windowWidth = 1;
            _windowHeight = 1;
            UpdatePerspectiveMatrix();
            UpdateViewMatrix();
        }

        void UpdateBackend(BackendChangedEvent e)
        {
            _useReverseDepth = e.GraphicsDevice.IsDepthRangeZeroToOne;
            _isClipSpaceYInverted = e.GraphicsDevice.IsClipSpaceYInverted;
            UpdatePerspectiveMatrix();
        }

        public float AspectRatio => _windowWidth / _windowHeight;
        public float Magnification { get; set; } // Ignored.

        public float Yaw { get => _yaw; set { _yaw = value; UpdateViewMatrix(); } }

        public float Pitch
        {
            get => _pitch;
            set
            {
                _pitch = Math.Clamp(value, -1.55f, 1.55f);
                UpdateViewMatrix();
            }
        }

        void WindowResized(float width, float height)
        {
            _windowWidth = width;
            _windowHeight = height;
            UpdatePerspectiveMatrix();
        }

        void UpdatePerspectiveMatrix()
        {
            _projectionMatrix = Util.CreatePerspective(
                _isClipSpaceYInverted,
                _useReverseDepth,
                FieldOfView,
                _windowWidth / _windowHeight,
                NearDistance,
                FarDistance);
        }

        void UpdateViewMatrix()
        {
            Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
            Vector3 lookDir = Vector3.Transform(-Vector3.UnitZ, lookRotation);
            _lookDirection = lookDir;
            _viewMatrix = Matrix4x4.CreateLookAt(_position, _position + _lookDirection, Vector3.UnitY);
        }

        public CameraInfo GetCameraInfo() => new CameraInfo
        {
            CameraPosition_WorldSpace = _position,
            CameraLookDirection = _lookDirection
        };
    }
}