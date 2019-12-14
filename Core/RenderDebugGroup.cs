using System;
using UAlbion.Api;
using Veldrid;

namespace UAlbion.Core
{
    public class RenderDebugGroup : IDisposable
    {
        readonly CommandList _cl;
        readonly string _name;

        public RenderDebugGroup(CommandList cl, string name)
        {
            _cl = cl;
            _name = name;
            CoreTrace.Log.StartDebugGroup(name);
            cl.PushDebugGroup(name);
        }

        public void Dispose()
        {
            _cl.PopDebugGroup();
            CoreTrace.Log.StopDebugGroup(_name);
        }
    }
}