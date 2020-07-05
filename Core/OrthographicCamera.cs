using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core.Events;

namespace UAlbion.Core
{
    public class OrthographicCamera : ServiceComponent<ICamera>, ICamera
    {
        Vector3 _position = new Vector3(0, 0, 1);
        Vector2 _windowSize = Vector2.One;
        Matrix4x4 _viewMatrix;
        Matrix4x4 _projectionMatrix;
        float _magnification = 2.0f; // TODO: Ensure this defaults to something sensible, and at some point lock it to a value that fits the gameplay and map design.

        public Matrix4x4 ViewMatrix => _viewMatrix;
        public Matrix4x4 ProjectionMatrix => _projectionMatrix;
        public Vector3 Position { get => _position; set { _position = value; UpdateViewMatrix(); } }
        public float Magnification { get => _magnification; set { _magnification = value; UpdatePerspectiveMatrix(); } }
        public Vector3 LookDirection { get; } = new Vector3(0, 0, -1f);
        public float FarDistance => 100f;
        public float FieldOfView => 1f;
        public float NearDistance => 0.1f;

        public float AspectRatio => _windowSize.X / _windowSize.Y;

        public OrthographicCamera()
        {
            OnAsync<ScreenCoordinateSelectEvent, Selection>(TransformSelect);
            On<SetCameraMagnificationEvent>(e =>
            {
                _magnification = e.Magnification;
                UpdatePerspectiveMatrix();
            });

            On<MagnifyEvent>(e =>
            {
                if (_magnification < 1.0f && e.Delta > 0)
                    _magnification = 0.0f;

                _magnification += e.Delta;

                if (_magnification < 0.5f)
                    _magnification = 0.5f;
                UpdatePerspectiveMatrix();
                Raise(new SetCameraMagnificationEvent(_magnification));
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
            _projectionMatrix.M11 = (2.0f * _magnification) / _windowSize.X;
            _projectionMatrix.M22 = (-2.0f * _magnification) / _windowSize.Y;
        }

        void UpdateViewMatrix()
        {
            _viewMatrix = Matrix4x4.Identity;
            _viewMatrix.M41 = -_position.X;
            _viewMatrix.M42 = -_position.Y;
        }

        public CameraInfo GetCameraInfo()
        {
            var clock = Resolve<IClock>();
            var settings = Resolve<IEngineSettings>();

            return new CameraInfo
            {
                WorldSpacePosition = _position,
                Resolution = _windowSize,
                Time = clock.ElapsedTime,
                Special1 = settings.Special1,
                Special2 = settings.Special2,
                EngineFlags = (uint)settings.Flags
            };
        }

        public Vector3 ProjectWorldToNorm(Vector3 worldPosition) 
            => Vector3.Transform(worldPosition + Vector3.UnitZ, ViewMatrix * ProjectionMatrix);
        public Vector3 UnprojectNormToWorld(Vector3 normPosition)
        {
            var totalMatrix = ViewMatrix * ProjectionMatrix;
            var inverse = totalMatrix.Inverse();
            return Vector3.Transform(normPosition + Vector3.UnitZ, inverse);
        }
    }
}
