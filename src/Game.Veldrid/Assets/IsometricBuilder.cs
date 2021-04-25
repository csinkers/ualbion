using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Visual;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Config;
using UAlbion.Game.Entities.Map3D;
using UAlbion.Game.Events;

namespace UAlbion.Game.Veldrid.Assets
{
    public class IsometricBuilder : Component
    {
        readonly FramebufferSource _framebufferSource;
        readonly IsometricLayout _layout;
        LabyrinthId _labId;
        IsometricMode _mode = IsometricMode.Floors;
        float _pitch;
        float _yaw = 45;
        int _width;
        int _height;
        int _tilesPerRow;
        int? _paletteId;

        public IsometricBuilder(FramebufferSource framebufferSource, int width, int height, int diamondHeight, int tilesPerRow)
        {
            _framebufferSource = framebufferSource ?? throw new ArgumentNullException(nameof(framebufferSource));
            _labId = Base.Labyrinth.Test1;
            _layout = AttachChild(new IsometricLayout());
            _width = width;
            _height = height;
            _pitch = ApiUtil.RadToDeg(MathF.Asin((float)diamondHeight / _width));
            _tilesPerRow = tilesPerRow;

            On<IsoYawEvent>(e => { _yaw += e.Delta; Update(); });
            On<IsoPitchEvent>(e => { _pitch += e.Delta; Update(); });
            On<IsoRowWidthEvent>(e =>
            {
                _tilesPerRow += e.Delta;
                if (_tilesPerRow < 1) _tilesPerRow = 1;
                Update();
            });
            On<IsoWidthEvent>(e =>
            {
                _width += e.Delta;
                if (_width < 1) _width = 1;
                Update();
            });
            On<IsoHeightEvent>(e =>
            {
                _height += e.Delta;
                if (_height < 1) _height = 1;
                Update();
            });
            On<IsoPaletteEvent>(e =>
            {
                if (_paletteId == null) _paletteId = e.Delta;
                else _paletteId += e.Delta;

                if (_paletteId <= 0) _paletteId = null;
                Info($"PalId: {_paletteId}");
                RecreateLayout();
            });
            On<IsoLabDeltaEvent>(e =>
            {
                _labId = new LabyrinthId(AssetType.Labyrinth, _labId.Id + e.Delta);
                Info($"LabId: {_labId} ({_labId.Id})");
                RecreateLayout();
            });
            On<IsoLabEvent>(e => { _labId = e.Id; RecreateLayout(); });
            On<IsoModeEvent>(e =>
            {
                _mode = e.Mode;
                RecreateLayout();
            });
        }

        public List<int>[] Build(LabyrinthData labyrinth, AssetInfo info, IsometricMode mode, IAssetManager assets)
        {
            if (labyrinth == null) throw new ArgumentNullException(nameof(labyrinth));
            _labId = labyrinth.Id;
            _mode = mode;
            
            _layout.Load(labyrinth, info, _mode, BuildProperties(), _paletteId, assets);
            int rows = (_layout.TileCount + _tilesPerRow - 1) / _tilesPerRow;
            _framebufferSource.Width = (uint)(_width * _tilesPerRow);
            _framebufferSource.Height = (uint)(_height * rows);
            Update();

            return mode switch
            {
                IsometricMode.Floors => _layout.FloorFrames,
                IsometricMode.Ceilings => _layout.CeilingFrames,
                IsometricMode.Walls => _layout.WallFrames,
                IsometricMode.Contents => _layout.ContentsFrames,
                _ => null
            };
        }

        protected override void Subscribed()
        {
            Raise(new InputModeEvent(InputMode.IsoBake));
            Raise(new EngineFlagEvent(FlagOperation.Set, EngineFlags.FlipDepthRange));
            Raise(new CameraMagnificationEvent(1.0f));
            Raise(new CameraPlanesEvent(0, 5000));
            // Raise(new EngineFlagEvent(FlagOperation.Set, EngineFlags.ShowBoundingBoxes));
            RecreateLayout();
        }

        void RecreateLayout()
        {
            _layout.Load(_labId, _mode, BuildProperties(), _paletteId);
            int rows = (_layout.TileCount + _tilesPerRow - 1) / _tilesPerRow;
            _framebufferSource.Width = (uint)(_width * _tilesPerRow);
            _framebufferSource.Height = (uint)(_height * rows);
            Update();
        }

        public DungeonTileMapProperties Properties => _layout.Properties;
        public LabyrinthId LabyrinthId => _labId;
        public IsometricMode Mode => _mode;
        public int TilesPerRow => _tilesPerRow;
        public float DiamondHeight => _width * MathF.Sin(MathF.Abs(PitchRads));
        public float SideLength => _width * MathF.Cos(YawRads);
        public float YHeight => (_height - DiamondHeight) / MathF.Cos(MathF.Abs(PitchRads));
        float YawRads => ApiUtil.DegToRad(_yaw);
        float PitchRads => ApiUtil.DegToRad(-_pitch);

        void Update() => _layout.Properties = BuildProperties();

        DungeonTileMapProperties BuildProperties(bool log = false)
        {
            _yaw = Math.Clamp(_yaw, -45.0f, 45.0f);
            _pitch = Math.Clamp(_pitch, -85.0f, 85.0f);

            int rows = (_layout.TileCount + _tilesPerRow - 1) / _tilesPerRow;
            var viewport = new Vector2(_width * _tilesPerRow, _height * rows);
            if (log)
            {
                Info($"{_tilesPerRow}x{rows} " +
                    $"Y:{(int)_yaw} P:{(int)_pitch} " +
                    $"{_width}x{_height} = {SideLength:N2}x{YHeight:N2} " +
                    $"DH:{DiamondHeight:N2} R:{DiamondHeight / _width:N2} " +
                    $"Total Dims: {viewport}");
            }

            var camera = Resolve<ICamera>();
            camera.Viewport = viewport;
            var topLeft = camera.UnprojectNormToWorld(new Vector3(-1, 1, -0.5f));

            return new DungeonTileMapProperties(
                new Vector3(SideLength, YHeight, SideLength),
                new Vector3(PitchRads, YawRads, 0),
                topLeft + new Vector3(_width, -_height, 0) / 2,
                _width * Vector3.UnitX,
                -_height * Vector3.UnitY,
                (uint)_tilesPerRow,
                0,
                0,
                1.0f); // y-scale
        }
    }
}