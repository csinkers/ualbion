﻿using System.Numerics;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Visual;

public interface ICamera : IComponent
{
    int Version { get; }
    Matrix4x4 ViewMatrix { get; }
    Matrix4x4 ProjectionMatrix { get; }
    Vector3 Position { get; set; }
    Vector3 LookDirection { get; }
    Vector2 Viewport { get; set; }
    float Yaw { get; set; }
    float Pitch { get; set; }
    float NearDistance { get; }
    float FarDistance { get; }
    float FieldOfView { get; }
    float AspectRatio { get; }
    float Magnification { get; }
    Vector3 ProjectWorldToNorm(Vector3 worldPosition);
    Vector3 UnprojectNormToWorld(Vector3 normPosition);
}