using UAlbion.Core;
using UAlbion.Core.Events;

namespace UAlbion.Game
{
    public class LayoutManager : Component
    {
        static readonly Handler[] Handlers =
        {
            new Handler<LayoutManager, RenderEvent>((x,e) => x.Render(e)), 
        };

        void Render(RenderEvent renderEvent)
        {
        }

        public LayoutManager() : base(Handlers) { }
    }
}
