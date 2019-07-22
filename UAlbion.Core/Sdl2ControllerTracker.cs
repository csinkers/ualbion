/*
 using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid.Sdl2;

namespace UAlbion.Core
{
    public class Sdl2ControllerTracker : IDisposable
    {
        readonly int _controllerIndex;
        readonly SDL_GameController _controller;

        public string ControllerName { get; }

        readonly Dictionary<SDL_GameControllerAxis, float> _axisValues = new Dictionary<SDL_GameControllerAxis, float>();
        readonly Dictionary<SDL_GameControllerButton, bool> _buttons = new Dictionary<SDL_GameControllerButton, bool>();

        public unsafe Sdl2ControllerTracker(int index)
        {
            _controller = Sdl2Native.SDL_GameControllerOpen(index);
            SDL_Joystick joystick = Sdl2Native.SDL_GameControllerGetJoystick(_controller);
            _controllerIndex = SDL_JoystickInstanceID(joystick);
            ControllerName = Marshal.PtrToStringUTF8((IntPtr)Sdl2Native.SDL_GameControllerName(_controller));
            Sdl2Events.Subscribe(ProcessEvent);
        }

        public float GetAxis(SDL_GameControllerAxis axis)
        {
            _axisValues.TryGetValue(axis, out float ret);
            return ret;
        }

        public bool IsPressed(SDL_GameControllerButton button)
        {
            _buttons.TryGetValue(button, out bool ret);
            return ret;
        }

        public static bool CreateDefault(out Sdl2ControllerTracker sct)
        {
            int jsCount = Sdl2Native.SDL_NumJoysticks();
            for (int i = 0; i < jsCount; i++)
            {
                if (Sdl2Native.SDL_IsGameController(i))
                {
                    sct = new Sdl2ControllerTracker(i);
                    return true;
                }
            }

            sct = null;
            return false;
        }

        void ProcessEvent(ref SDL_Event ev)
        {
            switch (ev.type)
            {
                case SDL_EventType.ControllerAxisMotion:
                    SDL_ControllerAxisEvent axisEvent = Unsafe.As<SDL_Event, SDL_ControllerAxisEvent>(ref ev);
                    if (axisEvent.which == _controllerIndex)
                    {
                        _axisValues[axisEvent.axis] = Normalize(axisEvent.value);
                    }
                    break;
                case SDL_EventType.ControllerButtonDown:
                case SDL_EventType.ControllerButtonUp:
                    SDL_ControllerButtonEvent buttonEvent = Unsafe.As<SDL_Event, SDL_ControllerButtonEvent>(ref ev);
                    if (buttonEvent.which == _controllerIndex)
                    {
                        _buttons[buttonEvent.button] = buttonEvent.state == 1;
                    }
                    break;
            }
        }

        float Normalize(short value)
        {
            return value < 0
                ? -(value / (float)short.MinValue)
                : (value / (float)short.MaxValue);
        }

        public void Dispose()
        {
            Sdl2Native.SDL_GameControllerClose(_controller);
        }
    }
}
*/
