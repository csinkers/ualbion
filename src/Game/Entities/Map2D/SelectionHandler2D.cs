using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Config;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Scenes;
using UAlbion.Game.Text;

namespace UAlbion.Game.Entities.Map2D;

public sealed class SelectionHandler2D : Component
{
    static readonly Vector3 Normal = Vector3.UnitZ;
    readonly LogicalMap2D _map;
    readonly MapRenderable2D _renderable;
    readonly MapTileHit _mapTileHit = new();
    readonly DebugMapTileHit _debugMapTileHit = new();
    Func<object, string> _formatChain;
    int _lastHighlightIndex;

    public SelectionHandler2D(LogicalMap2D map, MapRenderable2D renderable)
    {
        On<WorldCoordinateSelectEvent>(OnSelect);
        On<ShowMapMenuEvent>(_ => ShowMapMenu());
        On<UiRightClickEvent>(e =>
        {
            e.Propagating = false;
            Raise(new PushMouseModeEvent(MouseMode.RightButtonHeld));
        });

        _map = map ?? throw new ArgumentNullException(nameof(map));
        _renderable = renderable;
    }

    public event EventHandler<int> HighlightIndexChanged;
    protected override void Subscribed()
    {
        var assets = Resolve<IAssetManager>();
        var eventFormatter = new EventFormatter(assets.LoadStringSafe, _map.Id.ToMapText());
        _formatChain = x =>
        {
            var builder = new UnformattedScriptBuilder(false);
            eventFormatter.FormatChain(builder, (IEventNode)x);
            return builder.Build();
        };
    }

    void OnSelect(WorldCoordinateSelectEvent e)
    {
        float denominator = Vector3.Dot(Normal, e.Direction);
        if (Math.Abs(denominator) < 0.00001f)
            return;

        float t = Vector3.Dot(-e.Origin, Normal) / denominator;
        if (t < 0)
            return;

        Vector3 intersectionPoint = e.Origin + t * e.Direction;
        int x = (int)(intersectionPoint.X / _renderable.TileSize.X);
        int y = (int)(intersectionPoint.Y / _renderable.TileSize.Y);

        _mapTileHit.Tile = new Vector2(x, y);
        e.Selections.Add(new Selection(e.Origin, e.Direction, t, _mapTileHit));

        if (e.Debug)
        {
            _debugMapTileHit.Tile = new Vector2(x, y);
            _debugMapTileHit.IntersectionPoint = intersectionPoint;
            _debugMapTileHit.UnderlayTile = _map.GetUnderlay(x, y);
            _debugMapTileHit.OverlayTile = _map.GetOverlay(x, y);
            e.Selections.Add(new Selection(e.Origin, e.Direction, t, _debugMapTileHit));
        }
        e.Selections.Add(new Selection(e.Origin, e.Direction, t, this));

        if (e.Debug)
        {
            var zone = _map.GetZone(x, y);
            if (zone != null)
                e.Selections.Add(new Selection(e.Origin, e.Direction, t, zone));

            var chain = zone?.Chain;
            if (chain != null)
                e.Selections.Add(new Selection(e.Origin, e.Direction, t, zone.Node, _formatChain));
        }

        int highlightIndex = y * _map.Width + x;
        if (_lastHighlightIndex != highlightIndex)
        {
            HighlightIndexChanged?.Invoke(this, highlightIndex);
            _lastHighlightIndex = highlightIndex;
        }
    }

    void ShowMapMenu()
    {
        int x = _lastHighlightIndex % _map.Width;
        int y = _lastHighlightIndex / _map.Width;
        var window = Resolve<IGameWindow>();
        var camera = Resolve<ICameraProvider>().Camera;
        var tf = Resolve<ITextFormatter>();

        IText S(TextId textId) => tf.Center().Format(textId);
        var worldPosition = new Vector2(x, y) * _map.TileSize;
        var normPosition = camera.ProjectWorldToNorm(new Vector3(worldPosition, 0.0f));
        var uiPosition = window.NormToUi(normPosition.X, normPosition.Y);
        var heading = S(Base.SystemText.MapPopup_Environment);
        var options = new List<ContextMenuOption>();

        var zone = _map.GetOffsetZone(x, y);

        if (zone?.Chain != null && zone.Node != null)
        {
            if ((zone.Trigger & TriggerTypes.Examine) != 0)
            {
                options.Add(new ContextMenuOption(
                    S(Base.SystemText.MapPopup_Examine),
                    new TriggerMapTileEvent(TriggerType.Examine, zone.X, zone.Y),
                    ContextMenuGroup.Actions));
            }

            if ((zone.Trigger & TriggerTypes.Manipulate) != 0)
            {
                options.Add(new ContextMenuOption(
                    S(Base.SystemText.MapPopup_Manipulate),
                    new TriggerMapTileEvent(TriggerType.Manipulate, zone.X, zone.Y),
                    ContextMenuGroup.Actions));
            }

            if ((zone.Trigger & TriggerTypes.Take) != 0)
            {
                options.Add(new ContextMenuOption(
                    S(Base.SystemText.MapPopup_Take),
                    new TriggerMapTileEvent(TriggerType.Take, zone.X, zone.Y),
                    ContextMenuGroup.Actions));
            }

            if ((zone.Trigger & TriggerTypes.TalkTo) != 0)
            {
                options.Add(new ContextMenuOption(
                    S(Base.SystemText.MapPopup_TalkTo),
                    new TriggerMapTileEvent(TriggerType.TalkTo, zone.X, zone.Y),
                    ContextMenuGroup.Actions));
            }
        }

        // Check if map allows Rest

        options.Add(new ContextMenuOption(
            S(Base.SystemText.MapPopup_MainMenu),
            new PushSceneEvent(SceneId.MainMenu),
            ContextMenuGroup.System
        ));

        Raise(new ContextMenuEvent(uiPosition, heading, options));
    }
}

/*

    Headers:
        MapPopup_Environment

    Actions:
      x MapPopup_Examine
      x MapPopup_Manipulate
      x MapPopup_Take
      x MapPopup_TalkTo
        MapPopup_Rest
      x MapPopup_MainMenu
        MapPopup_Map (3D only)
        MapPopup_Wait (3D only)

        MapPopup_Blocked1
        MapPopup_Blocked2
        MapPopup_CannotCarryThatMuch <--- consequence of "Take"
        MapPopup_ItsTooDangerousHere <-- consequence of "Rest"
        MapPopup_NoSpaceLeft <--- consequence of "Take"
        MapPopup_Person
        MapPopup_ReallyRest <--- consequence of "Rest"
        MapPopup_ThesePeopleDoNotSpeakTheSameLanguage <-- consequence of "TalkTo"
        MapPopup_ThisItemDoesntWorkHere
        MapPopup_ThisPersonIsAsleep
        MapPopup_ThisPersonSpeaksALanguageLeaderDoesntUnderstand <-- consequence of "TalkTo"
        MapPopup_ThisWordDoesntWorkHere
        MapPopup_TooFarAway1
        MapPopup_TooFarAway2
        MapPopup_TooFarAwayToTalkTo <-- consequence of "TalkTo"
        MapPopup_TooFarAwayToTouch
        MapPopup_TravelOnFoot
        MapPopup_UseItem
        MapPopup_UseWhichItem
        MapPopup_WaitForHowManyHours <-- consequence of "Wait"
*/
