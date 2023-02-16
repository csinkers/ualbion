using System;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public sealed class DummyResourceSet : IResourceSetHolder
{
    public ResourceSet ResourceSet => throw new NotSupportedException();
    public static DummyResourceSet Instance { get; } = new();
    DummyResourceSet() { }
    public void Dispose() { }
}