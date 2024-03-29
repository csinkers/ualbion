﻿using System;

namespace UAlbion.Api.Visual;

#pragma warning disable CA1000 // Do not declare static members on generic types
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Never compared")]
public readonly ref struct ReadOnlyImageBuffer<T>
{
    public static ReadOnlyImageBuffer<T> Empty => new(0, 0, 0, ReadOnlySpan<T>.Empty);
    public ReadOnlyImageBuffer(int width, int height, int stride, ReadOnlySpan<T> buffer)
    {
        Width = width;
        Height = height;
        Stride = stride;
        Buffer = buffer;
    }

    public ReadOnlyImageBuffer(ReadOnlyImageBuffer<T> existing, ReadOnlySpan<T> buffer)
    {
        Width = existing.Width;
        Height = existing.Height;
        Stride = existing.Stride;
        Buffer = buffer;
    }

    public int Width { get; }
    public int Height { get; }
    public int Stride { get; }
    public ReadOnlySpan<T> Buffer { get; }
    public ReadOnlySpan<T> GetRow(int row) => Buffer.Slice(Stride * row, Width);
}
#pragma warning restore CA1000 // Do not declare static members on generic types