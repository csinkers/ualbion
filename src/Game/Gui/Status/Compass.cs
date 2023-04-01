using System;
using System.Numerics;
using UAlbion.Config;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets.Save;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Settings;
using UAlbion.Game.State;

namespace UAlbion.Game.Gui.Status;

public class Compass : Dialog
{
    const int TicksPerFrame = 6;
    const int FrameCount = 8;
    const int MarkerRadius = 10;
    static readonly (int, int) Position  = (7, 5);
    static readonly (int, int) Size  = (29, 29);
    static readonly Vector2 CenterRelative = new(15, 15);
    static readonly (int, int) MarkerSize = (6, 6);

    readonly UiSpriteElement _face;
    readonly UiSpriteElement _marker;
    readonly FixedPositionStacker _markerStacker;
    readonly FixedPositionStacker _mainStacker;
    byte _ticks;
    byte _frame;

    public Compass() : base(DialogPositioning.TopLeft)

    {
        On<FastClockEvent>(_ => Update());
        _face = new UiSpriteElement(AssetId.None);
        _marker = new UiSpriteElement(AssetId.None);
        _markerStacker = new FixedPositionStacker().Add(_marker, 0, 0);
        var layerStack = new LayerStacker(_face, _markerStacker);
        _mainStacker = AttachChild(new FixedPositionStacker()
            .Add(layerStack, Position.Item1, Position.Item2, Size.Item1, Size.Item2));
    }

    void Update()
    {
        bool active = ((Resolve<IGameState>()?.ActiveItems ?? 0) & ActiveItems.Compass) != 0;
        if (active)
        {
            _ticks++;
            if (_ticks == TicksPerFrame)
            {
                _frame++;
                _ticks = 0;
                if (_frame == FrameCount)
                    _frame = 0;
            }

            var language = Var(UserVars.Gameplay.Language);
            _face.Id = language switch
            {
                Base.Language.German => Base.CoreGfx.CompassDe,
                Base.Language.French => Base.CoreGfx.CompassFr,
                _ => Base.CoreGfx.CompassEn
            };

            _marker.Id = (_frame & 0x7) switch
            {
                0 => Base.CoreGfx.CompassDot0,
                1 => Base.CoreGfx.CompassDot1,
                2 => Base.CoreGfx.CompassDot2,
                3 => Base.CoreGfx.CompassDot3,
                4 => Base.CoreGfx.CompassDot4,
                5 => Base.CoreGfx.CompassDot5,
                6 => Base.CoreGfx.CompassDot6,
                _ => Base.CoreGfx.CompassDot7
            };

            var (mx, my) = CalculateMarkerPosition();
            _markerStacker.Move(_marker, mx, my);
        }

        _mainStacker.IsActive = active;
    }

    (int,int) CalculateMarkerPosition()
    {
        var camera = Resolve<ICameraProvider>().Camera as PerspectiveCamera;
        var angle = camera?.Yaw ?? 0;
        angle += (float)Math.PI / 2;
        var direction = new Vector2((float)Math.Cos(angle), -(float)Math.Sin(angle));
        var pos = CenterRelative + MarkerRadius * direction;
        return ((int)pos.X - MarkerSize.Item1 / 2, (int)pos.Y - MarkerSize.Item2 / 2);
    }
}
