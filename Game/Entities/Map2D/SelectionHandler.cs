using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Gui;
using UAlbion.Game.Scenes;

namespace UAlbion.Game.Entities.Map2D
{
    public sealed class SelectionHandler : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<SelectionHandler, WorldCoordinateSelectEvent>((x, e) => x.OnSelect(e)),
            H<SelectionHandler, RightClickEvent>((x, e) => x.OnRightClick())
        );


        static readonly Vector3 Normal = Vector3.UnitZ;
        readonly LogicalMap _map;
        readonly Vector2 _tileSize;
        int _lastHighlightIndex;

        public SelectionHandler(LogicalMap map, Vector2 tileSize) : base(Handlers)
        {
            _map = map ?? throw new ArgumentNullException(nameof(map));
            _tileSize = tileSize;
        }

        public event EventHandler<int> HighlightIndexChanged;

        void OnSelect(WorldCoordinateSelectEvent e)
        {
            float denominator = Vector3.Dot(Normal, e.Direction);
            if (Math.Abs(denominator) < 0.00001f)
                return;

            float t = Vector3.Dot(-e.Origin, Normal) / denominator;
            if (t < 0)
                return;

            Vector3 intersectionPoint = e.Origin + t * e.Direction;
            int x = (int)(intersectionPoint.X / _tileSize.X);
            int y = (int)(intersectionPoint.Y / _tileSize.Y);

            int highlightIndex = y * _map.Width + x;
            var underlayTile = _map.GetUnderlay(x, y);
            var overlayTile = _map.GetOverlay(x, y);
            var zones = _map.GetZones(x, y);

            e.RegisterHit(t, new MapTileHit(new Vector2(x, y), intersectionPoint));
            if (underlayTile != null) e.RegisterHit(t, underlayTile);
            if (overlayTile != null) e.RegisterHit(t, overlayTile);
            e.RegisterHit(t, this);

            foreach(var zone in zones)
            {
                e.RegisterHit(t, zone);
                HashSet<IEventNode> printedEvents = new HashSet<IEventNode>();
                var zoneEvent = zone.Event;
                while (zoneEvent != null && !printedEvents.Contains(zoneEvent))
                {
                    e.RegisterHit(t, zoneEvent);
                    printedEvents.Add(zoneEvent);
                    zoneEvent = zoneEvent.NextEvent;
                } 
            }

            if (_lastHighlightIndex != highlightIndex)
            {
                HighlightIndexChanged?.Invoke(this, highlightIndex);
                _lastHighlightIndex = highlightIndex;
            }
        }

        void OnRightClick()
        {
            int x = _lastHighlightIndex % _map.Width;
            int y = _lastHighlightIndex / _map.Width;
            var window = Resolve<IWindowManager>();
            var camera = Resolve<ICamera>();
            var assets = Resolve<IAssetManager>();
            var settings = Resolve<ISettings>();

            ITextSource S(SystemTextId textId) => new DynamicText(() =>
                {
                    var template = assets.LoadString(textId, settings.Gameplay.Language);
                    return new TextFormatter(assets, settings.Gameplay.Language).Centre().Format(template).Blocks;
                });

            var worldPosition = new Vector2(x, y) * _map.TileSize;
            var normPosition = camera.ProjectWorldToNorm(new Vector3(worldPosition, 0.0f));
            var uiPosition = window.NormToUi(new Vector2(normPosition.X, normPosition.Y));
            var heading = S(SystemTextId.MapPopup_Environment);
            var zones = _map.GetZones(x, y);

            var options = new List<ContextMenuOption>();
            foreach(var zone in zones)
            {
                if (zone.Trigger.HasFlag(TriggerType.Examine))
                {
                    options.Add(new ContextMenuOption(
                        S(SystemTextId.MapPopup_Examine),
                        new TriggerChainEvent(zone.Event, TriggerType.Examine, x, y),
                        ContextMenuGroup.Actions));
                }

                if (zone.Trigger.HasFlag(TriggerType.Manipulate))
                {
                    options.Add(new ContextMenuOption(
                        S(SystemTextId.MapPopup_Manipulate),
                        new TriggerChainEvent(zone.Event, TriggerType.Manipulate, x, y),
                        ContextMenuGroup.Actions));
                }

                if (zone.Trigger.HasFlag(TriggerType.Take))
                {
                    options.Add(new ContextMenuOption(
                        S(SystemTextId.MapPopup_Take),
                        new TriggerChainEvent(zone.Event, TriggerType.Take, x, y),
                        ContextMenuGroup.Actions));
                }

                if (zone.Trigger.HasFlag(TriggerType.TalkTo))
                {
                    options.Add(new ContextMenuOption(
                        S(SystemTextId.MapPopup_TalkTo),
                        new TriggerChainEvent(zone.Event, TriggerType.TalkTo, x, y),
                        ContextMenuGroup.Actions));
                }
            }

            // Check if map allows Rest

            options.Add(new ContextMenuOption(
                    S(SystemTextId.MapPopup_MainMenu),
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
