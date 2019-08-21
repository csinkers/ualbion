using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.Parsers;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities
{
    public class MapRenderable3D : Component
    {
        readonly MapData3D _mapData;
        readonly LabyrinthData _labyrinthData;

        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<MapRenderable3D, RenderEvent>((x, e) => x.Render(e)),
            new Handler<MapRenderable3D, UpdateEvent>((x, e) => x.Update()),
            new Handler<MapRenderable3D, SubscribedEvent>((x, e) => x.Subscribed())
        };

        public MapRenderable3D(MapData3D mapData, LabyrinthData labyrinthData) : base(Handlers)
        {
            _mapData = mapData;
            _labyrinthData = labyrinthData;
        }

        void Subscribed() { Raise(new LoadPalEvent((int)_mapData.PaletteId)); }

        void Update()
        {
        }

        void Render(RenderEvent e)
        {
        }
    }
}