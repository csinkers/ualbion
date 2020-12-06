using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Config;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Scenes;
using UAlbion.Game.Text;

namespace UAlbion.Game.Entities.Map2D
{
    public sealed class SelectionHandler2D : Component
    {
        static readonly Vector3 Normal = Vector3.UnitZ;
        readonly LogicalMap2D _map;
        readonly MapRenderable2D _renderable;
        int _lastHighlightIndex;

        public SelectionHandler2D(LogicalMap2D map, MapRenderable2D renderable)
        {
            OnAsync<WorldCoordinateSelectEvent, Selection>(OnSelect);
            On<ShowMapMenuEvent>(e => ShowMapMenu());
            On<UiRightClickEvent>(e =>
            {
                e.Propagating = false;
                Raise(new PushMouseModeEvent(MouseMode.RightButtonHeld));
            });

            _map = map ?? throw new ArgumentNullException(nameof(map));
            _renderable = renderable;
        }

        public event EventHandler<int> HighlightIndexChanged;

        bool OnSelect(WorldCoordinateSelectEvent e, Action<Selection> continuation)
        {
            float denominator = Vector3.Dot(Normal, e.Direction);
            if (Math.Abs(denominator) < 0.00001f)
                return false;

            float t = Vector3.Dot(-e.Origin, Normal) / denominator;
            if (t < 0)
                return false;

            Vector3 intersectionPoint = e.Origin + t * e.Direction;
            int x = (int)(intersectionPoint.X / _renderable.TileSize.X);
            int y = (int)(intersectionPoint.Y / _renderable.TileSize.Y);

            int highlightIndex = y * _map.Width + x;
            var underlayTile = _map.GetUnderlay(x, y);
            var overlayTile = _map.GetOverlay(x, y);

            continuation(new Selection(e.Origin, e.Direction, t, new MapTileHit(
                new Vector2(x, y),
                intersectionPoint,
                _renderable.GetWeakUnderlayReference(x, y),
                _renderable.GetWeakOverlayReference(x, y))));

            if (underlayTile != null) continuation(new Selection(e.Origin, e.Direction, t, underlayTile));
            if (overlayTile != null) continuation(new Selection(e.Origin, e.Direction, t, overlayTile));
            continuation(new Selection(e.Origin, e.Direction, t, this));

            var zone = _map.GetZone(x, y);
            if (zone != null)
                continuation(new Selection(e.Origin, e.Direction, t, zone));

            var chain = zone?.Chain;
            if (chain != null)
            {
                foreach (var zoneEvent in chain.Events)
                    continuation(new Selection(e.Origin, e.Direction, t, zoneEvent));
            }

            if (_lastHighlightIndex != highlightIndex)
            {
                HighlightIndexChanged?.Invoke(this, highlightIndex);
                _lastHighlightIndex = highlightIndex;
            }

            return true;
        }

        void ShowMapMenu()
        {
            int x = _lastHighlightIndex % _map.Width;
            int y = _lastHighlightIndex / _map.Width;
            var window = Resolve<IWindowManager>();
            var camera = Resolve<ICamera>();
            var tf = Resolve<ITextFormatter>();

            IText S(TextId textId) => tf.Center().Format(textId);
            var worldPosition = new Vector2(x, y) * _map.TileSize;
            var normPosition = camera.ProjectWorldToNorm(new Vector3(worldPosition, 0.0f));
            var uiPosition = window.NormToUi(normPosition.X, normPosition.Y);
            var heading = S(Base.SystemText.MapPopup_Environment);
            var options = new List<ContextMenuOption>();

            var zone = _map.GetZone(x, y);
            if (zone?.Chain != null && zone.Node != null)
            {
                if (zone.Trigger.HasFlag(TriggerTypes.Examine))
                {
                    options.Add(new ContextMenuOption(
                        S(Base.SystemText.MapPopup_Examine),
                        new TriggerChainEvent(zone.Chain, zone.Node, new EventSource(_map.Id, _map.Id.ToMapText(), TriggerTypes.Examine, x, y)),
                        ContextMenuGroup.Actions));
                }

                if (zone.Trigger.HasFlag(TriggerTypes.Manipulate))
                {
                    options.Add(new ContextMenuOption(
                        S(Base.SystemText.MapPopup_Manipulate),
                        new TriggerChainEvent(zone.Chain, zone.Node, new EventSource(_map.Id, _map.Id.ToMapText(), TriggerTypes.Manipulate, x, y)),
                        ContextMenuGroup.Actions));
                }

                if (zone.Trigger.HasFlag(TriggerTypes.Take))
                {
                    options.Add(new ContextMenuOption(
                        S(Base.SystemText.MapPopup_Take),
                        new TriggerChainEvent(zone.Chain, zone.Node, new EventSource(_map.Id, _map.Id.ToMapText(), TriggerTypes.Take, x, y)),
                        ContextMenuGroup.Actions));
                }

                if (zone.Trigger.HasFlag(TriggerTypes.TalkTo))
                {
                    options.Add(new ContextMenuOption(
                        S(Base.SystemText.MapPopup_TalkTo),
                        new TriggerChainEvent(zone.Chain, zone.Node, new EventSource(_map.Id, _map.Id.ToMapText(), TriggerTypes.TalkTo, x, y)),
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
        MapPopup_Map (3D only0
        MapPopup_Wait (3D only0

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
