using System;
using UAlbion.Api;
using Veldrid;

namespace UAlbion.Core.Veldrid
{
    public class RenderDebugGroup : IDisposable
    {
        readonly string _name;
        readonly CommandList _cl;

        public RenderDebugGroup(CommandList cl, string name)
        {
            _cl = cl;
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
}
