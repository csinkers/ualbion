using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game.State
{
    public class StateManager : Component, IStateManager
    {
        public IParty Party { get; set; }
        public int FrameCount { get; set; }
        public PaletteId PaletteId { get; set; }
        public DateTime Time { get; set; }
        public Vector3 CameraTilePosition => CameraPosition / TileSize;
        public Vector3 CameraPosition { get; set; }
        public Vector2 CameraDirection { get; set; }
        public float CameraMagnification { get; set; }
        public Vector3 TileSize { get; set; } = Vector3.One;

        static readonly Handler[] Handlers =
        {
            new Handler<StateManager, UpdateEvent>((x, e) => { x.FrameCount += e.Frames; }),
            new Handler<StateManager, SetTileSizeEvent>((x, e) => { x.TileSize = e.TileSize; }),
            new Handler<StateManager, LoadPaletteEvent>((x, e) => { x.PaletteId = (PaletteId)e.PaletteId; }),
            new Handler<StateManager, SetCameraPositionEvent>((x, e) => x.CameraPosition = e.Position),
            new Handler<StateManager, SetCameraDirectionEvent>((x, e) => x.CameraDirection = new Vector2(e.Yaw, e.Pitch)),
            new Handler<StateManager, SetCameraMagnificationEvent>((x, e) => x.CameraMagnification = e.Magnification),
        };

        public StateManager() : base(Handlers) { }
    }
}