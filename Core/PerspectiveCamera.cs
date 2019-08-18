using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core.Events;
using Veldrid;

namespace UAlbion.Core
{
    public class PerspectiveCamera : Component, ICamera
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<PerspectiveCamera, WindowResizedEvent>((x, e) => x.WindowResized(e.Width, e.Height)),
            new Handler<PerspectiveCamera, EngineUpdateEvent>((x, e) => x.Update(e.DeltaSeconds))
        };

        Matrix4x4 _viewMatrix;
        Matrix4x4 _projectionMatrix;

        Vector3 _position = new Vector3(0, 3, 0);
        Vector3 _lookDirection = new Vector3(0, -.3f, -1f);

        //readonly float _moveSpeed = 10.0f;
        //Vector2 _mousePressedPos;
        //bool _mousePressed = false;
        float _yaw;
        float _pitch;
        GraphicsDevice _gd;
        bool _useReverseDepth;
        float _windowWidth;
        float _windowHeight;
        //readonly Sdl2Window _window;

        public Matrix4x4 ViewMatrix => _viewMatrix;
        public Matrix4x4 ProjectionMatrix => _projectionMatrix;
        public Vector3 Position { get => _position; set { _position = value; UpdateViewMatrix(); } }
        public Vector3 LookDirection => _lookDirection;
        public float FieldOfView => 1f;
        public float NearDistance => 1f;
        public float FarDistance => 1000f;

        public PerspectiveCamera(GraphicsDevice gd, float windowWidth, float windowHeight) : base(Handlers)
        {
            _gd = gd;
            _useReverseDepth = gd.IsDepthRangeZeroToOne;
            _windowWidth = windowWidth;
            _windowHeight = windowHeight;
            UpdatePerspectiveMatrix();
            UpdateViewMatrix();
        }

        public void UpdateBackend(GraphicsDevice gd)
        {
            _gd = gd;
            _useReverseDepth = gd.IsDepthRangeZeroToOne;
            UpdatePerspectiveMatrix();
        }

        public float AspectRatio => _windowWidth / _windowHeight;
        public float Magnification { get; set; } // Ignored.

        public float Yaw { get => _yaw; set { _yaw = value; UpdateViewMatrix(); } }
        public float Pitch { get => _pitch; set { _pitch = value; UpdateViewMatrix(); } }

        public void Update(float deltaSeconds)
        {
            /*
            float sprintFactor = InputTracker.GetKey(Key.ControlLeft)
                ? 0.1f
                : InputTracker.GetKey(Key.ShiftLeft) ? 2.5f : 1f;

            Vector3 motionDir = Vector3.Zero;
            if (InputTracker.GetKey(Key.A))
            {
                motionDir += -Vector3.UnitX;
            }
            if (InputTracker.GetKey(Key.D))
            {
                motionDir += Vector3.UnitX;
            }
            if (InputTracker.GetKey(Key.W))
            {
                motionDir += -Vector3.UnitZ;
            }
            if (InputTracker.GetKey(Key.S))
            {
                motionDir += Vector3.UnitZ;
            }
            if (InputTracker.GetKey(Key.Q))
            {
                motionDir += -Vector3.UnitY;
            }
            if (InputTracker.GetKey(Key.E))
            {
                motionDir += Vector3.UnitY;
            }

            if (motionDir != Vector3.Zero)
            {
                Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
                motionDir = Vector3.Transform(Vector3.Normalize(motionDir), lookRotation);
                _position += motionDir * _moveSpeed * sprintFactor * deltaSeconds;
                UpdateViewMatrix();
            }

            if (!ImGui.GetIO().WantCaptureMouse
                && (InputTracker.GetMouseButton(MouseButton.Left) || InputTracker.GetMouseButton(MouseButton.Right)))
            {
                if (!_mousePressed)
                {
                    _mousePressed = true;
                    _mousePressedPos = InputTracker.MousePosition;
                    Sdl2Native.SDL_ShowCursor(0);
                    // Sdl2Native.SDL_SetWindowGrab(_window.SdlWindowHandle, true); 
                }
                Vector2 mouseDelta = _mousePressedPos - InputTracker.MousePosition;
                Sdl2Native.SDL_WarpMouseInWindow(_window.SdlWindowHandle, (int)_mousePressedPos.X, (int)_mousePressedPos.Y);
                Yaw += mouseDelta.X * 0.002f;
                Pitch += mouseDelta.Y * 0.002f;
            }
            else if(_mousePressed)
            {
                Sdl2Native.SDL_WarpMouseInWindow(_window.SdlWindowHandle, (int)_mousePressedPos.X, (int)_mousePressedPos.Y);
                // Sdl2Native.SDL_SetWindowGrab(_window.SdlWindowHandle, false);
                Sdl2Native.SDL_ShowCursor(1);
                _mousePressed = false;
            }

            Pitch = Math.Clamp(Pitch, -1.55f, 1.55f);
            UpdateViewMatrix();
            */
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
                _gd,
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