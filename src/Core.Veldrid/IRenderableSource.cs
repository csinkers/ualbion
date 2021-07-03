using System.Collections.Generic;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid
{
    public interface IRenderableSource
    {
        void Collect(List<IRenderable> renderables);
    }
}