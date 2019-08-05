using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core.Events;
using Veldrid;
using Veldrid.Sdl2;

namespace UAlbion.Core
{
    public class OrthographicCamera : Component, ICamera
    {
        static readonly IList<Handler> Handlers = new Handler[]
            {new Handler<OrthographicCamera, WindowResizedEvent>((x, e) =>
            {
                x._windowWidth = e.Width;
                x._windowHeight = e.Height;
                x.UpdatePerspectiveMatrix();
            })};

        Vector3 _position = new Vector3(0, 0, 1);
        Matrix4x4 _viewMatrix;
        Matrix4x4 _projectionMatrix;
        float _magnification = 1.0f;
        float _windowWidth;
        float _windowHeight;

        public Matrix4x4 ViewMatrix => _viewMatrix;
        public Matrix4x4 ProjectionMatrix => _projectionMatrix;
        public Vector3 Position { get => _position; set { _position = value; UpdateViewMatrix(); } }
        public float Magnification { get => _magnification; set { _magnification = value; UpdatePerspectiveMatrix(); } }
        public Vector3 LookDirection { get; } = new Vector3(0, 0, -1f);
        public float FarDistance => 100f;
        public float FieldOfView => 1f;
        public float NearDistance => 0.1f;

        public float AspectRatio => _windowWidth / _windowHeight;

        public OrthographicCamera(Sdl2Window window) : base(Handlers)
        {
            _windowWidth = window.Width;
            _windowHeight = window.Height;
            UpdatePerspectiveMatrix();
            UpdateViewMatrix();
        }

        public void UpdateBackend(GraphicsDevice gd) { UpdatePerspectiveMatrix(); }

        void UpdatePerspectiveMatrix()
        {
            _projectionMatrix = Matrix4x4.Identity;
            _projectionMatrix.M11 = (2.0f * _magnification) / _windowWidth;
            _projectionMatrix.M22 = (-2.0f * _magnification) / _windowHeight;
            _projectionMatrix.M41 = 0;
            _projectionMatrix.M42 = 0;

            Exchange?.Raise(new ProjectionMatrixChangedEvent(), this);
        }

        void UpdateViewMatrix()
        {
            _viewMatrix = Matrix4x4.Identity;
            _viewMatrix.M41 = -_position.X;
            _viewMatrix.M42 = -_position.Y;
            Exchange?.Raise(new ViewMatrixChangedEvent(), this);
        }

        public CameraInfo GetCameraInfo() => new CameraInfo
        {
            CameraPosition_WorldSpace = _position,
            CameraLookDirection = LookDirection
        };
    }
}