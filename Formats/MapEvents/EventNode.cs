using System;
using System.Diagnostics;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public enum MapEventType : byte
    {
        Script = 0,
        MapExit = 1,
        Door = 2,
        Chest = 3,
        Text = 4,
        Spinner = 5,
        Trap = 6,
        ChangeUsedItem = 7,
        DataChange = 8,
        ChangeIcon = 9,
        Encounter = 0xA,
        PlaceAction = 0xB,
        Query = 0xC,
        Modify = 0xD,
        Action = 0xE,
        Signal = 0xF,
        CloneAutomap = 0x10,
        Sound = 0x11,
        StartDialogue = 0x12,
        CreateTransport = 0x13,
        Execute = 0x14,
        RemovePartyMember = 0x15,
        EndDialogue = 0x16,
        Wipe = 0x17,
        PlayAnimation = 0x18,
        Offset = 0x19,
        Pause = 0x1A,
        SimpleChest = 0x1B,
        AskSurrender = 0x1C,
        DoScript = 0x1D,
        UnkFF = 0xFF // 3D only?
    }

    public interface IEventNode
    {
        int Id { get; }
        IMapEvent Event { get; }
        IEventNode NextEvent { get; set; }
    }

    public interface IBranchNode : IEventNode
    {
        IEventNode NextEventWhenFalse { get; set; }
    }

    public class BranchNode : EventNode, IBranchNode
    {
        public BranchNode(int id, IMapEvent @event, ushort? falseEventId) : base(id, @event)
        {
            NextEventWhenFalseId = falseEventId;
        }
        public override string ToString() => $"{Id}: if ({Event}) {{";
        public IEventNode NextEventWhenFalse { get; set; }
        public ushort? NextEventWhenFalseId { get; private set; }
    }

    public class EventNode : IEventNode
    {
        public const long SizeInBytes = 12;
        public override string ToString() => $"{Id}:{Event}";
        public int Id { get; }
        public IMapEvent Event { get; }
        public ushort? NextEventId { get; set; }
        public IEventNode NextEvent { get; set; }

        public EventNode(int id, IMapEvent @event)
        {
            Id = id;
            Event = @event;
        }

        public static EventNode Translate(EventNode node, ISerializer s, int id)
        {
            var initialPosition = s.Offset;
            MapEventType type = node?.Event?.EventType ?? MapEventType.UnkFF;
            s.UInt8("Type", () => (byte)type, x => type = (MapEventType)x);

            var @event = TranslateByType(node, s, type);

            if (@event is IQueryEvent query)
                node ??= new BranchNode(id, @event, query.FalseEventId);
            else
                node ??= new EventNode(id, @event);

            long expectedPosition = initialPosition + 10;
            long actualPosition = s.Offset;
            Debug.Assert(expectedPosition == actualPosition,
                $"Expected to have read {expectedPosition - initialPosition} bytes, but {actualPosition - initialPosition} have been read.");

            s.UInt16(nameof(NextEventId),
                () => node.NextEventId ?? 0xffff,
                x => node.NextEventId = x == 0xffff ? (ushort?)null : x);

            return node;
        }

        static IMapEvent TranslateByType(EventNode node, ISerializer s, MapEventType type) =>
            type switch // Individual parsers handle byte range [1,9]
            {
                MapEventType.Action => ActionEvent.Translate((ActionEvent)node?.Event, s),
                MapEventType.AskSurrender => AskSurrenderEvent.Translate((AskSurrenderEvent)node?.Event, s),
                MapEventType.ChangeIcon => ChangeIconEvent.Translate((ChangeIconEvent)node?.Event, s),
                MapEventType.ChangeUsedItem => ChangeUsedItemEvent.Translate((ChangeUsedItemEvent)node?.Event, s),
                MapEventType.Chest => OpenChestEvent.Translate((OpenChestEvent)node?.Event, s),
                MapEventType.CloneAutomap => CloneAutomapEvent.Translate((CloneAutomapEvent)node?.Event, s),
                MapEventType.CreateTransport => CreateTransportEvent.Translate((CreateTransportEvent)node?.Event, s),
                MapEventType.DataChange => DataChangeEvent.Translate((DataChangeEvent)node?.Event, s),
                MapEventType.DoScript => DoScriptEvent.Translate((DoScriptEvent)node?.Event, s),
                MapEventType.Door => DoorEvent.Translate((DoorEvent)node?.Event, s),
                MapEventType.Encounter => EncounterEvent.Translate((EncounterEvent)node?.Event, s),
                MapEventType.EndDialogue => EndDialogueEvent.Translate((EndDialogueEvent)node?.Event, s),
                MapEventType.Execute => ExecuteEvent.Translate((ExecuteEvent)node?.Event, s),
                MapEventType.MapExit => TeleportEvent.Translate((TeleportEvent)node?.Event, s),
                MapEventType.Modify => ModifyEvent.Translate((ModifyEvent)node?.Event, s),
                MapEventType.Offset => OffsetEvent.Translate((OffsetEvent)node?.Event, s),
                MapEventType.Pause => PauseEvent.Translate((PauseEvent)node?.Event, s),
                MapEventType.PlaceAction => PlaceActionEvent.Translate((PlaceActionEvent)node?.Event, s),
                MapEventType.PlayAnimation => PlayAnimationEvent.Translate((PlayAnimationEvent)node?.Event, s),
                MapEventType.Query => QueryEvent.Translate((QueryEvent)node?.Event, s),
                MapEventType.RemovePartyMember => RemovePartyMemberEvent.Translate((RemovePartyMemberEvent)node?.Event, s),
                MapEventType.Script => RunScriptEvent.Translate((RunScriptEvent)node?.Event, s),
                MapEventType.Signal => SignalEvent.Translate((SignalEvent)node?.Event, s),
                MapEventType.SimpleChest => SimpleChestEvent.Translate((SimpleChestEvent)node?.Event, s),
                MapEventType.Sound => SoundEvent.Translate((SoundEvent)node?.Event, s),
                MapEventType.Spinner => SpinnerEvent.Translate((SpinnerEvent)node?.Event, s),
                MapEventType.StartDialogue => StartDialogueEvent.Translate((StartDialogueEvent)node?.Event, s),
                MapEventType.Text => TextEvent.Translate((TextEvent)node?.Event, s),
                MapEventType.Trap => TrapEvent.Translate((TrapEvent)node?.Event, s),
                MapEventType.UnkFF => DummyMapEvent.Translate((DummyMapEvent)node?.Event, s),
                MapEventType.Wipe => WipeEvent.Translate((WipeEvent)node?.Event, s),
                _ => null
            };
    }
}
