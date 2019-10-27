using System.Numerics;
using UAlbion.Core.Events;

namespace UAlbion.Core
{
    public class OrthographicCamera : Component, ICamera
    {
        static readonly HandlerSet Handlers = new HandlerSet
        (
            H<OrthographicCamera, ScreenCoordinateSelectEvent>((x, e) => x.TransformSelect(e)),
            H<OrthographicCamera, MagnifyEvent>((x, e) =>
            {
                if (x._magnification < 1.0f && e.Delta > 0)
                    x._magnification = 0.0f;

                x._magnification += e.Delta;

                if (x._magnification < 0.5f)
                    x._magnification = 0.5f;
                x.UpdatePerspectiveMatrix();
                x.Raise(new SetCameraMagnificationEvent(x._magnification));
            }),

            H<OrthographicCamera, RenderEvent>((x, e) =>
            {
                var window = x.Resolve<IWindowManager>();
                var size = new Vector2(window.PixelWidth, window.PixelHeight);
                if (x._windowSize != size)
                {
                    x._windowSize = size;
                    x.UpdatePerspectiveMatrix();
                }
            })
        );

        void TransformSelect(ScreenCoordinateSelectEvent e)
        {
            var totalMatrix = ViewMatrix * ProjectionMatrix;
            var inverse = totalMatrix.Inverse();
            var normalisedScreenPosition = new Vector3(2 * e.Position.X / _windowSize.X - 1.0f, -2 * e.Position.Y / _windowSize.Y + 1.0f, 0.0f);
            var rayOrigin = Vector3.Transform(normalisedScreenPosition + Vector3.UnitZ, inverse);
            var rayDirection = Vector3.Transform(normalisedScreenPosition, inverse) - rayOrigin;
            rayOrigin = new Vector3(rayOrigin.X, rayOrigin.Y, rayOrigin.Z);
            Raise(new WorldCoordinateSelectEvent(rayOrigin, rayDirection, e.RegisterHit));
        }

        Vector3 _position = new Vector3(0, 0, 1);
        Matrix4x4 _viewMatrix;
        Matrix4x4 _projectionMatrix;
        float _magnification = 1.0f;
        Vector2 _windowSize = Vector2.One;

        public Matrix4x4 ViewMatrix => _viewMatrix;
        public Matrix4x4 ProjectionMatrix => _projectionMatrix;
        public Vector3 Position { get => _position; set { _position = value; UpdateViewMatrix(); } }
        public float Magnification { get => _magnification; set { _magnification = value; UpdatePerspectiveMatrix(); } }
        public Vector3 LookDirection { get; } = new Vector3(0, 0, -1f);
        public float FarDistance => 100f;
        public float FieldOfView => 1f;
        public float NearDistance => 0.1f;

        public float AspectRatio => _windowSize.X / _windowSize.Y;

        public OrthographicCamera() : base(Handlers)
        {
            UpdatePerspectiveMatrix();
            UpdateViewMatrix();
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

        public CameraInfo GetCameraInfo() => new CameraInfo
        {
            CameraPosition_WorldSpace = _position,
            CameraLookDirection = LookDirection
        };
    }
}