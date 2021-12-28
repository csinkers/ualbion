using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Config;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Entities.Map3D;
using UAlbion.Game.Events;
using VeldridGen.Interfaces;

namespace UAlbion.Game.Veldrid.Assets;

public class IsometricBuilder : Component
{
    public const int ContentsExpansionFactor = 4; // TODO: Work out the required size properly based on the largest sprites in the lab
    readonly IsometricLayout _layout;
    LabyrinthId _labId;
    IsometricMode _mode = IsometricMode.Floors;
    float _pitch;
    float _yaw = 45;
    int _width;
    int _height;
    int _tilesPerRow;
    int? _paletteId;

    public LabyrinthId LabyrinthId => _labId;
    public IsometricMode Mode => _mode;
    public int TilesPerRow => _tilesPerRow;
    public float DiamondHeight => _width * MathF.Sin(MathF.Abs(PitchRads));
    public float SideLength => _width * MathF.Cos(YawRads);
    public float YHeight => (_height - DiamondHeight) / MathF.Cos(MathF.Abs(PitchRads));
    float YawRads => ApiUtil.DegToRad(_yaw);
    float PitchRads => ApiUtil.DegToRad(-_pitch);
    public IFramebufferHolder Framebuffer { get; }
    int ExpansionFactor => _mode == IsometricMode.Contents ? ContentsExpansionFactor : 1;

    public IsometricBuilder(IFramebufferHolder framebuffer, int width, int height, int diamondHeight, int tilesPerRow)
    {
        Framebuffer = framebuffer ?? throw new ArgumentNullException(nameof(framebuffer));
        _labId = Base.Labyrinth.Test1;
        _layout = AttachChild(new IsometricLayout());
        _width = width;
        _height = height;
        _pitch = ApiUtil.RadToDeg(MathF.Asin((float)diamondHeight / _width));
        _tilesPerRow = tilesPerRow;

        On<IsoLabEvent>(e => { _labId = e.Id; RecreateLayout(); });
        On<IsoModeEvent>(e => { _mode = e.Mode; RecreateLayout(); });
        On<IsoYawEvent>(e => { _yaw += e.Delta; Update(); });
        On<IsoPitchEvent>(e => { _pitch += e.Delta; Update(); });
        On<IsoRowWidthEvent>(e =>
        {
            _tilesPerRow += e.Delta;
            if (_tilesPerRow < 1)
                _tilesPerRow = 1;
            Update();
        });
        On<IsoWidthEvent>(e =>
        {
            _width += e.Delta;
            if (_width < 1)
                _width = 1;
            Update();
        });
        On<IsoHeightEvent>(e =>
        {
            _height += e.Delta;
            if (_height < 1)
                _height = 1;
            Update();
        });
        On<IsoPaletteEvent>(e =>
        {
            _paletteId = (_paletteId ?? 0) + e.Delta;
            if (_paletteId <= 0)
                _paletteId = null;
            Info($"PalId: {_paletteId}");
            RecreateLayout();
        });
        On<IsoLabDeltaEvent>(e =>
        {
            var newId = _labId.Id + e.Delta;
            if (newId < 0)
                return;

            _labId = new LabyrinthId(AssetType.Labyrinth, newId);
            Info($"LabId: {_labId} ({_labId.Id})");
            RecreateLayout();
        });
    }

    void ResizeFramebuffer()
    {
        int rows = (_layout.TileCount + _tilesPerRow - 1) / _tilesPerRow;
        if (Framebuffer == null) 
            return;

        var mag = TryResolve<ICamera>()?.Magnification ?? 1.0f;
        Framebuffer.Width = (uint)(ExpansionFactor * _width * _tilesPerRow * mag);
        Framebuffer.Height = (uint)(ExpansionFactor * _height * rows * mag);
    }

    public List<int>[] Build(LabyrinthData labyrinth, AssetInfo info, IsometricMode mode, IAssetManager assets)
    {
        if (labyrinth == null) throw new ArgumentNullException(nameof(labyrinth));
        _labId = labyrinth.Id;
        _mode = mode;

        _layout.Load(labyrinth, info, _mode, BuildProperties(), _paletteId, assets);
        ResizeFramebuffer();
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
        Raise(new CameraJumpEvent(0, 0, -256));
        // Raise(new EngineFlagEvent(FlagOperation.Set, EngineFlags.ShowBoundingBoxes));
        RecreateLayout();
    }

    void RecreateLayout()
    {
        _layout.Load(_labId, _mode, BuildProperties(), _paletteId);
        ResizeFramebuffer();
        Update();
    }

    void Update() => _layout.Update(BuildProperties());

    TilemapRequest BuildProperties(bool log = false)
    {
        _yaw = Math.Clamp(_yaw, -45.0f, 45.0f);
        _pitch = Math.Clamp(_pitch, -85.0f, 85.0f);

        int rows = (_layout.TileCount + _tilesPerRow - 1) / _tilesPerRow;
        var viewport = new Vector2(ExpansionFactor * _width * _tilesPerRow, ExpansionFactor * _height * rows);
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

        return new TilemapRequest
        {
            Id = LabyrinthId,
            TileCount = _layout.TileCount,
            Scale = new Vector3(SideLength, YHeight, SideLength),
            Rotation = new Vector3(PitchRads, YawRads, 0),
            Origin = topLeft + new Vector3(_width, -_height, 0) * ExpansionFactor / 2,
            HorizontalSpacing = _width * ExpansionFactor * Vector3.UnitX,
            VerticalSpacing = -_height * ExpansionFactor * Vector3.UnitY,
            Width = (uint)_tilesPerRow
        };
    }
}