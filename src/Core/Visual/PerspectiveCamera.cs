using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core.Events;

namespace UAlbion.Core.Visual
{
    public class PerspectiveCamera : ServiceComponent<ICamera>, ICamera
    {
        Matrix4x4 _viewMatrix;
        Matrix4x4 _projectionMatrix;

        Vector3 _position = new Vector3(0, 0, 0);
        Vector3 _lookDirection = new Vector3(0, -.3f, -1f);
        Vector2 _windowSize = Vector2.One;

        float _yaw;
        float _pitch;
        bool _useReverseDepth;
        bool _isClipSpaceYInverted;
        bool _depthZeroToOne;

        public Matrix4x4 ViewMatrix => _viewMatrix;
        public Matrix4x4 ProjectionMatrix => _projectionMatrix;
        public Vector3 LookDirection => _lookDirection;
        public float FieldOfView { get; private set; }
        public float NearDistance { get; private set; } = 10f;
        public float FarDistance { get; private set; } = 512.0f * 256.0f * 2.0f;
        public bool LegacyPitch { get; }
        public float AspectRatio => _windowSize.X / _windowSize.Y;
        public float Magnification { get; set; } // Ignored.
        public float Yaw { get => _yaw; set { _yaw = value; UpdateViewMatrix(); } } // Radians

        public float Pitch // Radians
        {
            get => _pitch;
            set
            {
                _pitch = LegacyPitch
                    ? Math.Clamp(value, -0.48f, 0.48f)
                    : Math.Clamp(value, (float)-Math.PI / 2, (float)Math.PI / 2);

                if (LegacyPitch)
                    UpdatePerspectiveMatrix();
                else
                    UpdateViewMatrix();
            }
        }

        public Vector3 Position
        {
            get => _position;
            set { _position = value; UpdateViewMatrix(); }
        }

        public PerspectiveCamera(bool legacyPitch = false)
        {
            OnAsync<ScreenCoordinateSelectEvent, Selection>(TransformSelect);
            On<BackendChangedEvent>(_ => UpdateBackend());
            On<EngineFlagEvent>(_ => UpdateBackend());
            // BUG: This event is not received when the screen is resized while a 2D scene is active.
            On<WindowResizedEvent>(e => WindowResized(e.Width, e.Height));
            On<CameraPositionEvent>(e => Position = new Vector3(e.X, e.Y, e.Z));
            On<CameraDirectionEvent>(e =>
            {
                Yaw = ApiUtil.DegToRad(e.Yaw);
                Pitch = ApiUtil.DegToRad(e.Pitch);
            });
            On<CameraPlanesEvent>(e =>
            {
                NearDistance = e.Near; FarDistance = e.Far; 
                UpdatePerspectiveMatrix();
            });

            On<SetFieldOfViewEvent>(e =>
            {
                if (e.Degrees == null)
                {
                    Raise(new LogEvent(LogEvent.Level.Info, $"FOV {ApiUtil.RadToDeg(FieldOfView)}"));
                }
                else
                {
                    FieldOfView = ApiUtil.DegToRad(e.Degrees.Value);
                    UpdatePerspectiveMatrix();
                }
            });

            LegacyPitch = legacyPitch;
            FieldOfView = (float)(Math.PI * (legacyPitch ? 60 : 80) / 180);
        }

        protected override void Subscribed()
        {
            _windowSize = Resolve<IWindowManager>().Size;
            UpdateBackend();
            UpdateViewMatrix();
            base.Subscribed();
        }

        bool TransformSelect(ScreenCoordinateSelectEvent e, Action<Selection> continuation)
        {
            var normalisedScreenPosition = new Vector3(2 * e.Position.X / _windowSize.X - 1.0f, -2 * e.Position.Y / _windowSize.Y + 1.0f, 0.0f);
            var rayOrigin = UnprojectNormToWorld(normalisedScreenPosition + Vector3.UnitZ);
            var rayDirection = UnprojectNormToWorld(normalisedScreenPosition) - rayOrigin;
            RaiseAsync(new WorldCoordinateSelectEvent(rayOrigin, rayDirection), continuation);
            return true;
        }

        void UpdateBackend()
        {
            var settings = TryResolve<IEngineSettings>();
            var e = TryResolve<IEngine>();
            if (settings != null && e != null)
            {
                _useReverseDepth = (settings?.Flags & EngineFlags.FlipDepthRange) == EngineFlags.FlipDepthRange;
                _depthZeroToOne = e.IsDepthRangeZeroToOne;
                _isClipSpaceYInverted = (settings?.Flags & EngineFlags.FlipYSpace) == EngineFlags.FlipYSpace
                    ? !e.IsClipSpaceYInverted
                    : e.IsClipSpaceYInverted;
            }


            UpdatePerspectiveMatrix();
        }

        void WindowResized(float width, float height)
        {
            _windowSize.X = width;
            _windowSize.Y = height;
            UpdatePerspectiveMatrix();
        }

        void UpdatePerspectiveMatrix()
        {
            _projectionMatrix = LegacyPitch
                ? MatrixUtil.CreateLegacyPerspective(
                    _isClipSpaceYInverted,
                    _useReverseDepth,
                    _depthZeroToOne,
                    FieldOfView,
                    AspectRatio,
                    NearDistance,
                    FarDistance,
                    Pitch)
                : MatrixUtil.CreatePerspective(
                    _isClipSpaceYInverted,
                    _useReverseDepth,
                    _depthZeroToOne,
                    FieldOfView,
                    AspectRatio,
                    NearDistance,
                    FarDistance);
        }

        void UpdateViewMatrix()
        {
            Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, LegacyPitch ? 0f : Pitch, 0f);
            _lookDirection = Vector3.Transform(-Vector3.UnitZ, lookRotation);
            _viewMatrix = Matrix4x4.CreateLookAt(_position, _position + _lookDirection, Vector3.UnitY);
        }

        public CameraInfo GetCameraInfo()
        {
            var clock = TryResolve<IClock>();
            var settings = TryResolve<IEngineSettings>();

            return new CameraInfo
            {
                WorldSpacePosition = _position,
                CameraPitch = Pitch,
                CameraYaw = Yaw,
                Resolution = _windowSize,
                Time = clock?.ElapsedTime ?? 0,
                Special1 = settings?.Special1 ?? 0,
                Special2 = settings?.Special2 ?? 0,
                EngineFlags = (uint?)settings?.Flags ?? 0
            };
        }

        public Vector3 ProjectWorldToNorm(Vector3 worldPosition)
        {
            var v = Vector4.Transform(new Vector4(worldPosition, 1.0f), ViewMatrix * ProjectionMatrix);
            return new Vector3(v.X / v.W, v.Y / v.W, v.Z / v.W);
        }

        public Vector3 UnprojectNormToWorld(Vector3 normPosition)
        {
            var v = Vector4.Transform(new Vector4(normPosition, 1.0f), (ViewMatrix * ProjectionMatrix).Inverse());
            return new Vector3(v.X / v.W, v.Y / v.W, v.Z / v.W);
        }
    }
}
