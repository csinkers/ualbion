using System;
using System.Numerics;

namespace UAlbion.Core;

public class Transform
{
    Vector3 _position;
    Quaternion _rotation = Quaternion.Identity;
    Vector3 _scale = Vector3.One;

    public Vector3 Position { get => _position; set { _position = value; TransformChanged?.Invoke(); } }
    public Quaternion Rotation { get => _rotation; set { _rotation = value; TransformChanged?.Invoke(); } }
    public Vector3 Scale { get => _scale; set { _scale = value; TransformChanged?.Invoke(); } }

    public event Action TransformChanged;

    public Vector3 Forward => Vector3.Transform(-Vector3.UnitZ, _rotation);

    public Matrix4x4 GetTransformMatrix()
    {
        return Matrix4x4.CreateScale(_scale)
               * Matrix4x4.CreateFromQuaternion(_rotation)
               * Matrix4x4.CreateTranslation(Position);
    }
}