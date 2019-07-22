using System;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace UAlbion.Core
{
    public class OrthographicCamera : ICamera
    {
        Matrix4x4 _viewMatrix;
        Matrix4x4 _projectionMatrix;
        Vector3 _position = new Vector3(0, 0, 1);

        GraphicsDevice _gd;
        bool _useReverseDepth;
        float _windowWidth;
        float _windowHeight;

        public event Action<Matrix4x4> ProjectionChanged;
        public event Action<Matrix4x4> ViewChanged;

        public Matrix4x4 ViewMatrix => _viewMatrix;
        public Matrix4x4 ProjectionMatrix => _projectionMatrix;
        public Vector3 Position { get => _position; set { _position = value; UpdateViewMatrix(); } }
        public Vector3 LookDirection { get; } = new Vector3(0, 0, -1f);
        public float FarDistance => 100f;
        public float FieldOfView => 1f;
        public float NearDistance => 0.1f;

        public float AspectRatio => _windowWidth / _windowHeight;

        public OrthographicCamera(GraphicsDevice gd, Sdl2Window window)
        {
            _gd = gd;
            _useReverseDepth = gd.IsDepthRangeZeroToOne;
            _windowWidth = window.Width;
            _windowHeight = window.Height;
            UpdatePerspectiveMatrix();
            UpdateViewMatrix();
        }

        public void Attach(EventExchange exchange)
        {
            exchange.Subscribe<WindowResizedEvent>(this);
        }

        public void Receive(IEvent gameEvent, object sender)
        {
            switch (gameEvent)
            {
                case WindowResizedEvent e:
                    _windowWidth = e.Width;
                    _windowHeight = e.Height;
                    UpdatePerspectiveMatrix();
                    break;
            }
        }

        public void UpdateBackend(GraphicsDevice gd)
        {
            _gd = gd;
            _useReverseDepth = gd.IsDepthRangeZeroToOne;
            UpdatePerspectiveMatrix();
        }

        void UpdatePerspectiveMatrix()
        {
            _projectionMatrix = Util.CreateOrtho(
                _gd,
                _useReverseDepth,
                0, _windowWidth,
                0, _windowHeight,
                NearDistance, FarDistance);

            ProjectionChanged?.Invoke(_projectionMatrix);
        }

        void UpdateViewMatrix()
        {
            _viewMatrix = Matrix4x4.CreateLookAt(_position, _position + LookDirection, Vector3.UnitY);
            ViewChanged?.Invoke(_viewMatrix);
        }

        public CameraInfo GetCameraInfo() => new CameraInfo
        {
            CameraPosition_WorldSpace = _position,
            CameraLookDirection = LookDirection
        };
    }
}