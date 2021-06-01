using System;
using UAlbion.Core;
using UAlbion.Core.Visual;

namespace UAlbion.TestCommon
{
    public class MockFactory : ICoreFactory
    {
        public IDisposable CreateRenderDebugGroup(IRendererContext context, string name)
            => new MockDisposable();

        public ISceneGraph CreateSceneGraph()
            => new MockSceneGraph();
    }
}