using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    public abstract class MapEvent : Event, IMapEvent
    {
        public abstract MapEventType EventType { get; }
        public static IMapEvent Serdes(IMapEvent e, ISerializer s, TextId textSourceId, AssetMapping mapping)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            var initialPosition = s.Offset;
            s.Begin();
            var type = s.EnumU8("Type", e?.EventType ?? MapEventType.UnkFf);
            e = type switch // Individual parsers handle byte range [1,9]
            {
                MapEventType.Action => ActionEvent.Serdes((ActionEvent)e, mapping, s),
                MapEventType.AskSurrender => AskSurrenderEvent.Serdes((AskSurrenderEvent)e, s),
                MapEventType.ChangeIcon => ChangeIconEvent.Serdes((ChangeIconEvent)e, s),
                MapEventType.ChangeUsedItem => ChangeUsedItemEvent.Serdes((ChangeUsedItemEvent)e, mapping, s),
                MapEventType.Chest => ChestEvent.Serdes((ChestEvent)e, mapping, s, textSourceId),
                MapEventType.CloneAutomap => CloneAutomapEvent.Serdes((CloneAutomapEvent)e, mapping, s),
                MapEventType.CreateTransport => CreateTransportEvent.Serdes((CreateTransportEvent)e, s),
                MapEventType.DataChange => DataChangeEvent.Serdes((DataChangeEvent)e, mapping, s),
                MapEventType.Door => DoorEvent.Serdes((DoorEvent)e, mapping, s, textSourceId),
                MapEventType.Encounter => EncounterEvent.Serdes((EncounterEvent)e, s),
                MapEventType.EndDialogue => EndDialogueEvent.Serdes((EndDialogueEvent)e, s),
                MapEventType.Execute => ExecuteEvent.Serdes((ExecuteEvent)e, s),
                MapEventType.MapExit => TeleportEvent.Serdes((TeleportEvent)e, mapping, s),
                MapEventType.Modify => ModifyEvent.BaseSerdes((ModifyEvent)e, mapping, s),
                MapEventType.Offset => OffsetEvent.Serdes((OffsetEvent)e, s),
                MapEventType.Pause => PauseEvent.Serdes((PauseEvent)e, s),
                MapEventType.PlaceAction => PlaceActionEvent.Serdes((PlaceActionEvent)e, s),
                MapEventType.PlayAnimation => PlayAnimationEvent.Serdes((PlayAnimationEvent)e, mapping, s),
                MapEventType.Query => QueryEvent.Serdes((QueryEvent)e, mapping, s, textSourceId),
                MapEventType.RemovePartyMember => RemovePartyMemberEvent.Serdes((RemovePartyMemberEvent)e, mapping, s),
                MapEventType.Script => DoScriptEvent.Serdes((DoScriptEvent)e, mapping, s),
                MapEventType.Signal => SignalEvent.Serdes((SignalEvent)e, s),
                MapEventType.SimpleChest => SimpleChestEvent.Serdes((SimpleChestEvent)e, mapping, s),
                MapEventType.Sound => SoundEvent.Serdes((SoundEvent)e, mapping, s),
                MapEventType.Spinner => SpinnerEvent.Serdes((SpinnerEvent)e, s),
                MapEventType.StartDialogue => StartDialogueEvent.Serdes((StartDialogueEvent)e, mapping, s),
                MapEventType.Text => TextEvent.Serdes((TextEvent)e, mapping, s, textSourceId),
                MapEventType.Trap => TrapEvent.Serdes((TrapEvent)e, s),
                MapEventType.Wipe => WipeEvent.Serdes((WipeEvent)e, s),
                _ => DummyMapEvent.Serdes((DummyMapEvent)e, s, type)
            };
            s.End();
            if (e is IBranchingEvent)
                s.Assert(s.Offset - initialPosition == 8, "Query events should always be 8 bytes");
            else
                s.Assert(s.Offset - initialPosition == 10, "Non-query map events should always be 10 bytes");
            return e;
        }
    }
}
