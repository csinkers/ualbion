using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Objects;
using UAlbion.Core.Textures;
using UAlbion.Formats.Parsers;
using UAlbion.Game.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public class Map : Component
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<Map, RenderEvent>((x, e) => x.Render(e)),
            new Handler<Map, SubscribedEvent>((x, e) => x.Raise(new LoadPalEvent((int) x.GetPalette())))
        };

        readonly Map2D _map;
        readonly SpriteRenderer.MultiSprite _underlay;
        readonly SpriteRenderer.MultiSprite _overlay;

        public Map(Assets assets, MapDataId mapId) : base(Handlers)
        {
            _map = assets.LoadMap2D(mapId);
            var tileset = assets.LoadTexture((IconGraphicsId)_map.TilesetId);

            _underlay = new SpriteRenderer.MultiSprite(new SpriteRenderer.SpriteKey(tileset, (int) DrawLayer.Underlay))
            {
                Instances = new SpriteRenderer.InstanceData[_map.Width * _map.Height]
            };

            _overlay = new SpriteRenderer.MultiSprite(new SpriteRenderer.SpriteKey(tileset, (int)DrawLayer.Overlay))
            {
                Instances = new SpriteRenderer.InstanceData[_map.Width * _map.Height]
            };

            tileset.GetSubImageDetails(0, out _, out var tileSize, out _);
            var tilesetSize = new Vector2(tileset.Width, tileset.Height);
            tileSize = tileSize * tilesetSize; // Convert from normalised coordinates to pixels

            unsafe
            {
                for (int j = 0; j < _map.Height; j++)
                {
                    for (int i = 0; i < _map.Width; i++)
                    {
                        tileset.GetSubImageDetails(_map.Underlay[j * _map.Width + i], out var texPosition, out var texSize, out var layer);
                        fixed (SpriteRenderer.InstanceData* instance = &_underlay.Instances[j * _map.Width + i])
                        {
                            instance->Offset = new Vector2(i, j) * tileSize;
                            instance->Size = tileSize;

                            instance->TexPosition = texPosition;
                            instance->TexSize = texSize;
                            instance->TexLayer = layer;

                            instance->Flags = tileset is EightBitTexture ? SpriteFlags.UsePalette : 0;
                        }

                        tileset.GetSubImageDetails(_map.Overlay[j * _map.Width + i], out texPosition, out texSize, out layer);
                        fixed (SpriteRenderer.InstanceData* instance = &_overlay.Instances[j * _map.Width + i])
                        {
                            instance->Offset = new Vector2(i, j) * tileSize;
                            instance->Size = tileSize;

                            instance->TexPosition = texPosition;
                            instance->TexSize = texSize;
                            instance->TexLayer = layer;

                            instance->Flags = tileset is EightBitTexture ? SpriteFlags.UsePalette : 0;
                        }
                    }
                }
            }
        }

        public PaletteId GetPalette() => (PaletteId)_map.PaletteId;

        void Render(RenderEvent e)
        {
            e.Add(_underlay);

            foreach (var npc in _map.Npcs)
            {
                var npcSprite = new SpriteDefinition<LargeNpcId>(
                    (LargeNpcId)npc.ObjectNumber,
                    0,
                    new Vector2(npc.Waypoints[0].X, npc.Waypoints[0].Y), 
                    (int)DrawLayer.Characters1,
                    0);

                e.Add(npcSprite);
            }

            e.Add(_overlay);
        }
    }
}