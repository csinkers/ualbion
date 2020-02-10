using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core.Events;

namespace UAlbion.Core
{
    public class PerspectiveCamera : Component, ICamera
    {
        static readonly HandlerSet Handlers = new HandlerSet
        (
            H<PerspectiveCamera, BackendChangedEvent>((x, e) => x.UpdateBackend(e)),
            // BUG: This event is not received when the screen is resized while a 2D scene is active.
            H<PerspectiveCamera, WindowResizedEvent> ((x, e) => x.WindowResized(e.Width, e.Height)),
            H<PerspectiveCamera, SetCameraDirectionEvent>((x, e) => { x.Yaw = e.Yaw; x.Pitch = e.Pitch; }),
            H<PerspectiveCamera, SetFieldOfViewEvent>((x, e) =>
            {
                if(e.Degrees == null)
                {
                    x.Raise(new LogEvent(LogEvent.Level.Info, $"FOV {Util.RadToDeg(x.FieldOfView)}"));
                }
                else
                {
                    x.FieldOfView = Util.DegToRad(e.Degrees.Value);
                    x.UpdatePerspectiveMatrix();
                }
            })
        );

        Matrix4x4 _viewMatrix;
        Matrix4x4 _projectionMatrix;

        Vector3 _position = new Vector3(0, 0, 0);
        Vector3 _lookDirection = new Vector3(0, -.3f, -1f);
        Vector2 _windowSize = Vector2.One;

        float _yaw;
        float _pitch;
        bool _useReverseDepth;
        bool _isClipSpaceYInverted;

        public Matrix4x4 ViewMatrix => _viewMatrix;
        public Matrix4x4 ProjectionMatrix => _projectionMatrix;

        public Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                UpdateViewMatrix();
                //Raise(new SetCameraPositionEvent(_position));
            }
        }

        public Vector3 LookDirection => _lookDirection;
        public float FieldOfView { get; private set; } = (float)(Math.PI * 80 / 180);
        public float NearDistance => 10f;
        public float FarDistance => 512.0f * 256.0f * 2.0f;

        public PerspectiveCamera() : base(Handlers)
        {
            UpdatePerspectiveMatrix();
            UpdateViewMatrix();
        }

        void UpdateBackend(BackendChangedEvent e)
        {
            var settings = Resolve<IEngineSettings>();
            _useReverseDepth = settings?.Flags.HasFlag(EngineFlags.FlipDepthRange) == true
                ? !e.GraphicsDevice.IsDepthRangeZeroToOne
                : e.GraphicsDevice.IsDepthRangeZeroToOne;

            _isClipSpaceYInverted = (settings?.Flags.HasFlag(EngineFlags.FlipYSpace) == true)
                ? !e.GraphicsDevice.IsClipSpaceYInverted 
                : e.GraphicsDevice.IsClipSpaceYInverted;
;
            UpdatePerspectiveMatrix();
        }

        public float AspectRatio => _windowSize.X / _windowSize.Y;
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
            _windowSize.X = width;
            _windowSize.Y = height;
            UpdatePerspectiveMatrix();
        }

        void UpdatePerspectiveMatrix()
        {
            _projectionMatrix = Util.CreatePerspective(
                _isClipSpaceYInverted,
                _useReverseDepth,
                FieldOfView,
                AspectRatio,
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

        public CameraInfo GetCameraInfo()
        {
            var clock = Resolve<IClock>();
            var settings = Resolve<IEngineSettings>();

            return new CameraInfo
            {
                WorldSpacePosition = _position,
                CameraPitch = Pitch,
                CameraYaw = Yaw,
                Resolution = _windowSize,
                Time = clock.ElapsedTime,
                Special1 = settings.Special1,
                Special2 = settings.Special2,
                EngineFlags = (uint)settings.Flags
            };
        }
    }
}