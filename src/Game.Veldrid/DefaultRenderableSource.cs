using System;
using System.Collections.Generic;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Veldrid
{
    public class AdhocRenderableSource : IRenderableSource
    {
        readonly IEnumerable<IRenderable> _renderables;
        public AdhocRenderableSource(IEnumerable<IRenderable> renderables) 
            => _renderables = renderables ?? throw new ArgumentNullException(nameof(renderables));

        public void Collect(List<IRenderable> renderables)
        {
            if (renderables == null) throw new ArgumentNullException(nameof(renderables));
            renderables.AddRange(_renderables);
        }
    }
    // public class DefaultRenderableSource : Component, IRenderableSource
    // {
    //     public void Collect(List<IRenderable> list)
    //     {
    //         if (list == null) throw new ArgumentNullException(nameof(list));

    //         TryResolve<ISkyboxManager>()?.Collect(list);
    //         ((IRenderableSource)Resolve<IEtmManager>()).Collect(list);
    //         Resolve<ISpriteManager>().Collect(list);
    //         DebugGuiRenderable.Instance.Collect(list);
    //     }
    // }
}