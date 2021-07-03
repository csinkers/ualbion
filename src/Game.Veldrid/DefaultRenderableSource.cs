using System;
using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Veldrid
{
    public class DefaultRenderableSource : Component, IRenderableSource
    {
        public void Collect(List<IRenderable> list)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));

            var skybox = (Skybox)TryResolve<ISkybox>();
            if (skybox != null)
                list.Add(skybox);

            var etmSource = (IRenderableSource)Resolve<IEtmManager>();
            etmSource.Collect(list);

            foreach (var batch in Resolve<ISpriteManager>().Batches)
                list.Add(batch);

            list.Add(DebugGuiRenderable.Instance);
        }
    }
}