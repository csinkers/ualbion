using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core.Events;
using Veldrid;

namespace UAlbion.Core
{
    public class OrthographicCamera : Component, ICamera
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<OrthographicCamera, BeginFrameEvent>((x, e) => x._movementDirection = Vector2.Zero),
            new Handler<OrthographicCamera, EngineCameraMoveEvent>((x, e) => x._movementDirection += new Vector2(e.X, e.Y)),
            new Handler<OrthographicCamera, ScreenCoordinateSelectEvent>((x, e) => x.TransformSelect(e)),
            new Handler<OrthographicCamera, MagnifyEvent>((x, e) =>
            {
                x._magnification += e.Delta;
                if (x._magnification < 1.0f)
                    x._magnification = 1.0f;
                x.UpdatePerspectiveMatrix();
            }),
            new Handler<OrthographicCamera, EngineUpdateEvent>((x, e) =>
            {
                x._position +=  new Vector3(x._movementDirection.X, x._movementDirection.Y, 0) * e.DeltaSeconds;
                x.UpdateViewMatrix();
            }),
            new Handler<OrthographicCamera, WindowResizedEvent>((x, e) =>
            {
                x.WindowWidth = e.Width;
                x.WindowHeight = e.Height;
                x.UpdatePerspectiveMatrix();
            })
        };

        void TransformSelect(ScreenCoordinateSelectEvent e)
        {
            var totalMatrix = ViewMatrix * ProjectionMatrix;
            var inverse = totalMatrix.Inverse();
            var normalisedScreenPosition = new Vector3(2 * e.Position.X / WindowWidth - 1.0f, -2 * e.Position.Y / WindowHeight + 1.0f, 0.0f);
            var rayOrigin = Vector3.Transform(normalisedScreenPosition + Vector3.UnitZ, inverse);
            var rayDirection = Vector3.Transform(normalisedScreenPosition, inverse) - rayOrigin;
            rayOrigin = new Vector3(rayOrigin.X, rayOrigin.Y, rayOrigin.Z);
            Raise(new WorldCoordinateSelectEvent(rayOrigin, rayDirection, e.RegisterHit));
        }

        Vector3 _position = new Vector3(0, 0, 1);
        Matrix4x4 _viewMatrix;
        Matrix4x4 _projectionMatrix;
        float _magnification = 1.0f;
        Vector2 _movementDirection;
        public float WindowWidth { get; private set; }
        public float WindowHeight { get; private set; }

        public Matrix4x4 ViewMatrix => _viewMatrix;
        public Matrix4x4 ProjectionMatrix => _projectionMatrix;
        public Vector3 Position { get => _position; set { _position = value; UpdateViewMatrix(); } }
        public float Magnification { get => _magnification; set { _magnification = value; UpdatePerspectiveMatrix(); } }
        public Vector3 LookDirection { get; } = new Vector3(0, 0, -1f);
        public float FarDistance => 100f;
        public float FieldOfView => 1f;
        public float NearDistance => 0.1f;

        public float AspectRatio => WindowWidth / WindowHeight;

        public OrthographicCamera() : base(Handlers)
        {
            WindowWidth = 1;
            WindowHeight = 1;
            UpdatePerspectiveMatrix();
            UpdateViewMatrix();
        }

        public void UpdateBackend(GraphicsDevice gd) { UpdatePerspectiveMatrix(); }

        void UpdatePerspectiveMatrix()
        {
            _projectionMatrix = Matrix4x4.Identity;
            _projectionMatrix.M11 = (2.0f * _magnification) / WindowWidth;
            _projectionMatrix.M22 = (-2.0f * _magnification) / WindowHeight;
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