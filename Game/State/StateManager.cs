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
        class GameState : IState
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
        }

        readonly GameState _state = new GameState();

        static readonly Handler[] Handlers = 
        {
            new Handler<StateManager, UpdateEvent>((x, e) => { x._state.FrameCount += e.Frames; }),
            new Handler<StateManager, SetTileSizeEvent>((x, e) => { x._state.TileSize = e.TileSize; }),
            new Handler<StateManager, LoadPaletteEvent>((x, e) => { x._state.PaletteId = (PaletteId)e.PaletteId; }),
            new Handler<StateManager, SetCameraPositionEvent>((x, e) => x._state.CameraPosition = e.Position),
            new Handler<StateManager, SetCameraDirectionEvent>((x, e) => x._state.CameraDirection = new Vector2(e.Yaw, e.Pitch)),
            new Handler<StateManager, SetCameraMagnificationEvent>((x, e) => x._state.CameraMagnification = e.Magnification),
        };

        public StateManager() : base(Handlers) { }
        public IState Get() { return _state; }
    }
}