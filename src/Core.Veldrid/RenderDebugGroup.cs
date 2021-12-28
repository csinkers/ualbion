using System;
using UAlbion.Api;
using Veldrid;

namespace UAlbion.Core.Veldrid;

public sealed class RenderDebugGroup : IDisposable
{
    readonly string _name;
    readonly CommandList _cl;

    public RenderDebugGroup(CommandList cl, string name)
    {
        _cl = cl ?? throw new ArgumentNullException(nameof(cl));
        _name = name;
        CoreTrace.Log.StartDebugGroup(name);
        _cl.PushDebugGroup(name);
    }

    public void Dispose()
    {
        _cl.PopDebugGroup();
        CoreTrace.Log.StopDebugGroup(_name);
    }
}