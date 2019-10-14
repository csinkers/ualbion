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
        public int FrameCount { get; private set; }
        public PaletteId PaletteId { get; private set; }
        public DateTime Time { get; set; }
        public Vector3 CameraTilePosition => CameraPosition / TileSize;
        public Vector3 CameraPosition { get; set; }
        public Vector2 CameraDirection { get; private set; }
        public float CameraMagnification { get; private set; }
        public Vector3 TileSize { get; private set; } = Vector3.One;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<StateManager, UpdateEvent>((x, e) => { x.FrameCount += e.Frames; }),
            H<StateManager, SetTileSizeEvent>((x, e) => { x.TileSize = e.TileSize; }),
            H<StateManager, LoadPaletteEvent>((x, e) => { x.PaletteId = e.PaletteId; }),
            H<StateManager, SetCameraDirectionEvent>((x, e) => x.CameraDirection = new Vector2(e.Yaw, e.Pitch)),
            H<StateManager, SetCameraMagnificationEvent>((x, e) => x.CameraMagnification = e.Magnification)
        );

        public StateManager() : base(Handlers) { }
    }
}