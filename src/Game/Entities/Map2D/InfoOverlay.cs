using System;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Input;

namespace UAlbion.Game.Entities.Map2D
{
    public class InfoOverlay : Component, IRenderable
    {
        readonly LogicalMap2D _logicalMap;
        readonly byte[] _tiles;
        InfoOverlayFlags _flags;

        public InfoOverlay(LogicalMap2D logicalMap)
        {
            _logicalMap = logicalMap ?? throw new ArgumentNullException(nameof(logicalMap));
            // 1 tile padding all around so we don't have to worry about edge conditions in the shader
            _tiles = new byte[Width * Height]; 
            _logicalMap.Dirty += (sender, args) =>
            {
                if (args.Type != IconChangeType.Trigger) 
                    return;

                int index = _logicalMap.Index(args.X, args.Y);
                var zone = _logicalMap.GetZone(index);
                var triggers = zone?.Trigger ?? 0;
                _tiles[Index(args.X, args.Y)] = BuildTile(triggers);
                BufferDirty = true;
            };
            On<BeginFrameEvent>(_ => _flags = 0);
            On<CursorModeEvent>(e 
                => _flags |= e.Mode switch
                {
                    CursorMode.Examine => InfoOverlayFlags.VerbExamine,
                    CursorMode.Manipulate => InfoOverlayFlags.VerbManipulate,
                    CursorMode.Talk => InfoOverlayFlags.VerbTalk,
                    CursorMode.Take => InfoOverlayFlags.VerbTake,
                    _ => 0
                });

            On<EngineUpdateEvent>(e =>
            {
                ExamineOpacity = Ease(ExamineOpacity, e.DeltaSeconds, (_flags & InfoOverlayFlags.VerbExamine) != 0);
                ManipulateOpacity = Ease(ManipulateOpacity, e.DeltaSeconds, (_flags & InfoOverlayFlags.VerbManipulate) != 0);
                TalkOpacity = Ease(TalkOpacity, e.DeltaSeconds, (_flags & InfoOverlayFlags.VerbTalk) != 0);
                TakeOpacity = Ease(TakeOpacity, e.DeltaSeconds, (_flags & InfoOverlayFlags.VerbTake) != 0);
            });
        }

        static float Ease(float current, float delta, bool active) 
            => Math.Clamp(current + 4.0f * (active ? delta : -delta), 0, 1.0f);

        public string Name => "Map Info Overlay";
        public DrawLayer RenderOrder => DrawLayer.Info;
        public int PipelineId => 0;
        public ReadOnlySpan<byte> Tiles => new ReadOnlySpan<byte>(_tiles);
        public bool BufferDirty { get; set; }
        public int Width => _logicalMap.Width;
        public int Height => _logicalMap.Height;
        public int TileWidth => (int)_logicalMap.TileSize.X;
        public int TileHeight => (int)_logicalMap.TileSize.Y;
        public float ExamineOpacity { get; private set; }
        public float ManipulateOpacity { get; private set; }
        public float TalkOpacity { get; private set; }
        public float TakeOpacity { get; private set; }
        protected override void Subscribed()
        {
            Resolve<IEngine>()?.RegisterRenderable(this);

            int index = 0;
            for (int j = 0; j < _logicalMap.Height; j++)
            {
                for (int i = 0; i < _logicalMap.Width; i++)
                {
                    var zone = _logicalMap.GetZone(index);
                    var triggers = zone?.Trigger ?? 0;
                    _tiles[Index(i, j)] = BuildTile(triggers);
                    index++;
                }
            }

            BufferDirty = true;
        }

        protected override void Unsubscribed() => Resolve<IEngine>()?.UnregisterRenderable(this);

        int Index(int i, int j) => j * Width + i;

        static byte BuildTile(TriggerTypes triggers) => (byte)(
              ((triggers & TriggerTypes.Examine)    != 0 ? InfoOverlayFlags.VerbExamine    : 0)
            | ((triggers & TriggerTypes.Manipulate) != 0 ? InfoOverlayFlags.VerbManipulate : 0)
            | ((triggers & TriggerTypes.TalkTo)     != 0 ? InfoOverlayFlags.VerbTalk       : 0)
            | ((triggers & TriggerTypes.Take)       != 0 ? InfoOverlayFlags.VerbTake       : 0)
            | (InfoOverlayFlags)0xf0);
    }
}
