using System;
using System.Numerics;
using System.Text;
using System.Threading;
using UAlbion.Api;
using UAlbion.Api.Visual;

namespace UAlbion.Core;

public static class CoreUtil
{
    public static bool IsDebug =>
#if DEBUG
        true;
#else
            false;
#endif

    public static float[] GetFullScreenQuadVerts(bool isClipSpaceYInverted) =>
        isClipSpaceYInverted
            ? new float[]
            {
                -1, -1, 0, 0,
                1, -1, 1, 0,
                1,  1, 1, 1,
                -1,  1, 0, 1
            }
            : new float[]
            {
                -1,  1, 0, 0,
                1,  1, 1, 0,
                1, -1, 1, 1,
                -1, -1, 0, 1
            };

    public static ArrayTexture<T> BuildTransposedTexture<T>(IReadOnlyTexture<T> texture) where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(texture);

        var rotatedPixels = new T[texture.Width * texture.Height];
        ApiUtil.TransposeImage(texture.Width, texture.Height,
            texture.PixelData,
            new Span<T>(rotatedPixels));

        return new ArrayTexture<T>(
            texture.Id,
            texture.Name + "Rotated",
            texture.Height, texture.Width,
            texture.ArrayLayers,
            rotatedPixels,
            new[] { new Region(
                Vector2.Zero,
                new Vector2(texture.Height, texture.Width),
                new Vector2(texture.Height, texture.Width),
                0)
            });
    }

    public static uint UpdateFlag(uint flags, FlagOperation operation, uint flag)
    {
        switch (operation)
        {
            case FlagOperation.Set: return flags | flag;
            case FlagOperation.Clear: return flags & ~flag;
            case FlagOperation.Toggle: return flags ^ flag;
            default: return flags;
        }
    }

    public static float UpdateValue(float value, ValueOperation operation, float argument)
    {
        switch (operation)
        {
            case ValueOperation.Set: return argument;
            case ValueOperation.Add: return value + argument;
            case ValueOperation.Mult: return value * argument;
            default: return value;
        }
    }

    public static bool IsCriticalException(Exception e) => e switch
    {
        OutOfMemoryException _ => true,
        ThreadAbortException _ => true,
        IndexOutOfRangeException _ => true,
        AccessViolationException _ => true,
        NullReferenceException _ => true,
        _ => false
    };

    public static string WordWrap(string s, int maxLine)
    {
        if (s == null || s.Length <= maxLine)
            return s;

        int n = 0;
        var sb = new StringBuilder();
        foreach (var c in s)
        {
            n = c == '\n' ? 0 : n + 1;

            sb.Append(c);
            if (n == maxLine)
            {
                sb.AppendLine();
                n = 0;
            }
        }

        return sb.ToString();
    }
}