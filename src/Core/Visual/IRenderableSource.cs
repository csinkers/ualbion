using System.Collections.Generic;

namespace UAlbion.Core.Visual
{
    public interface IRenderableSource
    {
        void Collect(List<IRenderable> renderables);
    }
}