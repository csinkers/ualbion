using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core.Events;

namespace UAlbion.Core.Visual
{
    public class OrthographicCamera : ServiceComponent<ICamera>, ICamera
    {
        readonly bool _yAxisIncreasesDownTheScreen;
        Vector3 _position = new Vector3(0, 0, 0);
        Vector3 _lookDirection = new Vector3(0, 0, -1f);
        Vector2 _windowSize = Vector2.One;
        Matrix4x4 _projectionMatrix;
        float _magnification = 1.0f; // TODO: Ensure this defaults to something sensible, and at some point lock it to a value that fits the gameplay and map design.
        float _yaw;
        float _pitch;
        float _farDistance = 1;

        public Matrix4x4 ViewMatrix { get; private set; }
        public Matrix4x4 ProjectionMatrix => _projectionMatrix;
        public Vector3 Position { get => _position; set { _position = value; UpdateViewMatrix(); } }
        public float Magnification { get => _magnification; private set { _magnification = value; UpdatePerspectiveMatrix(); } }
        public Vector3 LookDirection => _lookDirection;
        public float FieldOfView => 1f;

        public float NearDistance => 0;
        public float FarDistance { get => _farDistance; private set { _farDistance = value; UpdatePerspectiveMatrix(); } } 
        public float Yaw { get => _yaw; private set { _yaw = value; UpdateViewMatrix(); } } // Radians
        public float Pitch { get => _pitch; private set { _pitch = Math.Clamp(value, (float)-Math.PI / 2, (float)Math.PI / 2); UpdateViewMatrix(); } } // Radians
        public float AspectRatio => _windowSize.X / _windowSize.Y;

        public OrthographicCamera(bool yAxisIncreasesDownTheScreen = true)
        {
            _yAxisIncreasesDownTheScreen = yAxisIncreasesDownTheScreen;
            OnAsync<ScreenCoordinateSelectEvent, Selection>(TransformSelect);
            On<BackendChangedEvent>(_ => UpdatePerspectiveMatrix());
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
                UpdatePerspectiveMatrix();
                Raise(new CameraMagnificationEvent(_magnification));
            });

            On<RenderEvent>(e =>
            {
                var window = Resolve<IWindowManager>();
                var size = new Vector2(window.PixelWidth, window.PixelHeight);
                if (_windowSize != size)
                {
                    _windowSize = size;
                    UpdatePerspectiveMatrix();
                }
            });
        }

        protected override void Subscribed()
        {
            base.Subscribed();
            UpdatePerspectiveMatrix();
            UpdateViewMatrix();
        }

        bool TransformSelect(ScreenCoordinateSelectEvent e, Action<Selection> continuation)
        {
            var normalisedScreenPosition = new Vector3(2 * e.Position.X / _windowSize.X - 1.0f, -2 * e.Position.Y / _windowSize.Y + 1.0f, 0.0f);
            var rayOrigin = UnprojectNormToWorld(normalisedScreenPosition + Vector3.UnitZ);
            var rayDirection = UnprojectNormToWorld(normalisedScreenPosition) - rayOrigin;
            RaiseAsync(new WorldCoordinateSelectEvent(rayOrigin, rayDirection), continuation);
            return true;
        }

        void UpdatePerspectiveMatrix()
        {
            _projectionMatrix = Matrix4x4.Identity;
            _projectionMatrix.M11 = 2.0f * _magnification / _windowSize.X;
            _projectionMatrix.M22 = 2.0f * _magnification / _windowSize.Y;
            _projectionMatrix.M33 = 1 / FarDistance;

            var e = TryResolve<IEngine>();
            if (e != null && _yAxisIncreasesDownTheScreen != e.IsClipSpaceYInverted)
                _projectionMatrix.M22 = -_projectionMatrix.M22;
        }

        void UpdateViewMatrix()
        {
            Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
            _lookDirection = Vector3.Transform(-Vector3.UnitZ, lookRotation);
            ViewMatrix = Matrix4x4.CreateTranslation(-_position) * Matrix4x4.CreateFromQuaternion(lookRotation);
        }

        public CameraInfo GetCameraInfo()
        {
            var clock = TryResolve<IClock>();
            var settings = TryResolve<IEngineSettings>();

            return new CameraInfo
            {
                WorldSpacePosition = _position,
                Resolution = _windowSize,
                Time = clock?.ElapsedTime ?? 0,
                Special1 = settings?.Special1 ?? 0,
                Special2 = settings?.Special2 ?? 0,
                EngineFlags = (uint?)settings?.Flags ?? 0
            };
        }

        public Vector3 ProjectWorldToNorm(Vector3 worldPosition) => Vector3.Transform(worldPosition, ViewMatrix * ProjectionMatrix) - Vector3.UnitZ;
        public Vector3 UnprojectNormToWorld(Vector3 normPosition)
        {
            var totalMatrix = ViewMatrix * ProjectionMatrix;
            var inverse = totalMatrix.Inverse();
            return Vector3.Transform(normPosition + Vector3.UnitZ, inverse);
        }
    }
}
