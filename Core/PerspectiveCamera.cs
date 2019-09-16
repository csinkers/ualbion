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
            new Handler<PerspectiveCamera, WindowResizedEvent> ((x, e) => x.WindowResized(e.Width, e.Height)),
            new Handler<PerspectiveCamera, SetCameraPositionEvent>((x, e) => x.Position = e.Position),
            new Handler<PerspectiveCamera, SetCameraDirectionEvent>((x, e) => { x.Yaw = e.Yaw; x.Pitch = e.Pitch; }),
        };

        Matrix4x4 _viewMatrix;
        Matrix4x4 _projectionMatrix;

        Vector3 _position = new Vector3(0, 0, 0);
        Vector3 _lookDirection = new Vector3(0, -.3f, -1f);

        float _yaw;
        float _pitch;
        bool _useReverseDepth;
        bool _isClipSpaceYInverted;
        float _windowWidth;
        float _windowHeight;

        public Matrix4x4 ViewMatrix => _viewMatrix;
        public Matrix4x4 ProjectionMatrix => _projectionMatrix;

        public Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                UpdateViewMatrix();
                Raise(new SetCameraPositionEvent(_position));
            }
        }

        public Vector3 LookDirection => _lookDirection;
        public float FieldOfView => 1f;
        public float NearDistance => 10f;
        public float FarDistance => 512.0f * 256.0f * 2.0f;

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