using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    [DebuggerDisplay("{ToString()}")]
    public class EventNode : IEventNode
    {
        bool DirectSequence => (Next?.Id ?? Id + 1) == Id + 1;
        public override string ToString() => $"{(DirectSequence ? " " : "#")}{Id}=>{Next?.Id.ToString(CultureInfo.InvariantCulture) ?? "!"}: {Event}";
        public ushort Id { get; set; }
        public IEvent Event { get; }
        public IEventNode Next { get; set; }
        public EventNode(ushort id, IEvent @event)
        {
            Id = id;
            Event = @event;
        }

        public virtual void Unswizzle(IList<EventNode> nodes)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            if (!(Next is DummyEventNode dummy)) 
                return;

            if (dummy.Id >= nodes.Count)
            {
                ApiUtil.Assert($"Invalid event id: {Id} links to {dummy.Id}, but the set only contains {nodes.Count} events");
                Next = null;
            }
            else Next = nodes[dummy.Id];
        }

        public static EventNode Serdes(ushort id, EventNode node, ISerializer s, bool useEventText, ushort textSourceId)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            var initialPosition = s.Offset;
            var mapEvent = node?.Event as MapEvent;
            MapEventType type = (MapEventType)s.UInt8("Type", (byte)(mapEvent?.EventType ?? MapEventType.UnkFf));

            var @event = SerdesByType(mapEvent, s, type, useEventText, textSourceId);
            if (@event is IBranchingEvent)
            {
                node ??= new BranchNode(id, @event);
                var branch = (BranchNode)node;
                ushort? falseEventId = s.Transform<ushort, ushort?>(
                    nameof(branch.NextIfFalse),
                    branch.NextIfFalse?.Id,
                    S.UInt16,
                    MaxToNullConverter.Instance);

                if(falseEventId != null && branch.NextIfFalse == null)
                    branch.NextIfFalse = new DummyEventNode(falseEventId.Value);
            }
            else
                node ??= new EventNode(id, @event);

            ushort? nextEventId = s.Transform<ushort, ushort?>(nameof(node.Next), node.Next?.Id, S.UInt16, MaxToNullConverter.Instance);
            if (nextEventId != null && node.Next == null)
                node.Next = new DummyEventNode(nextEventId.Value);

            long expectedPosition = initialPosition + 12;
            long actualPosition = s.Offset;
            ApiUtil.Assert(expectedPosition == actualPosition,
                $"Expected to have read {expectedPosition - initialPosition} bytes, but {actualPosition - initialPosition} have been read.");

            return node;
        }

        static IMapEvent SerdesByType(IEvent e, ISerializer s, MapEventType type, bool useEventText, ushort textSourceId) =>
            type switch // Individual parsers handle byte range [1,9]
            {
                MapEventType.Action => ActionEvent.Serdes((ActionEvent)e, s),
                MapEventType.AskSurrender => AskSurrenderEvent.Serdes((AskSurrenderEvent)e, s),
                MapEventType.ChangeIcon => ChangeIconEvent.Serdes((ChangeIconEvent)e, s),
                MapEventType.ChangeUsedItem => ChangeUsedItemEvent.Serdes((ChangeUsedItemEvent)e, s),
                MapEventType.Chest => ChestEvent.Serdes((ChestEvent)e, s, useEventText ? AssetType.EventText : AssetType.MapText, textSourceId),
                MapEventType.CloneAutomap => CloneAutomapEvent.Serdes((CloneAutomapEvent)e, s),
                MapEventType.CreateTransport => CreateTransportEvent.Serdes((CreateTransportEvent)e, s),
                MapEventType.DataChange => DataChangeEvent.Serdes((DataChangeEvent)e, s),
                MapEventType.DoScript => DoScriptEvent.Serdes((DoScriptEvent)e, s),
                MapEventType.Door => DoorEvent.Serdes((DoorEvent)e, s, useEventText ? AssetType.EventText : AssetType.MapText, textSourceId),
                MapEventType.Encounter => EncounterEvent.Serdes((EncounterEvent)e, s),
                MapEventType.EndDialogue => EndDialogueEvent.Serdes((EndDialogueEvent)e, s),
                MapEventType.Execute => ExecuteEvent.Serdes((ExecuteEvent)e, s),
                MapEventType.MapExit => TeleportEvent.Serdes((TeleportEvent)e, s),
                MapEventType.Modify => ModifyEvent.Serdes((ModifyEvent)e, s),
                MapEventType.Offset => OffsetEvent.Serdes((OffsetEvent)e, s),
                MapEventType.Pause => PauseEvent.Serdes((PauseEvent)e, s),
                MapEventType.PlaceAction => PlaceActionEvent.Serdes((PlaceActionEvent)e, s),
                MapEventType.PlayAnimation => PlayAnimationEvent.Serdes((PlayAnimationEvent)e, s),
                MapEventType.Query => QueryEvent.Serdes((IQueryEvent)e, s, useEventText ? AssetType.EventText : AssetType.MapText, textSourceId),
                MapEventType.RemovePartyMember => RemovePartyMemberEvent.Serdes((RemovePartyMemberEvent)e, s),
                MapEventType.Script => RunScriptEvent.Serdes((RunScriptEvent)e, s),
                MapEventType.Signal => SignalEvent.Serdes((SignalEvent)e, s),
                MapEventType.SimpleChest => SimpleChestEvent.Serdes((SimpleChestEvent)e, s),
                MapEventType.Sound => SoundEvent.Serdes((SoundEvent)e, s),
                MapEventType.Spinner => SpinnerEvent.Serdes((SpinnerEvent)e, s),
                MapEventType.StartDialogue => StartDialogueEvent.Serdes((StartDialogueEvent)e, s),
                MapEventType.Text => useEventText
                        ? EventTextEvent.Serdes((BaseTextEvent)e, s, (EventSetId)textSourceId)
                        : MapTextEvent.Serdes((BaseTextEvent)e, s, (MapDataId)textSourceId),
                MapEventType.Trap => TrapEvent.Serdes((TrapEvent)e, s),
                MapEventType.Wipe => WipeEvent.Serdes((WipeEvent)e, s),
                _ => DummyMapEvent.Serdes((DummyMapEvent)e, s, type)
            };
    }
}

