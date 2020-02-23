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

        class ConvertMaxToNull : IConverter<ushort, ushort?>
        {
            public static readonly ConvertMaxToNull Instance = new ConvertMaxToNull();
            private ConvertMaxToNull() { }
            public ushort ToPersistent(ushort? memory) => memory ?? 0xffff;
            public ushort? ToMemory(ushort persistent) => persistent == 0xffff ? (ushort?)null : persistent;
        }

        public static EventNode Serdes(int id, EventNode node, ISerializer s)
        {
            var initialPosition = s.Offset;
            MapEventType type = (MapEventType)s.UInt8("Type", (byte)(node?.Event?.EventType ?? MapEventType.UnkFF));

            var @event = SerdesByType(node, s, type);
            if (@event is IQueryEvent query)
                node ??= new BranchNode(id, @event, query.FalseEventId);
            else
                node ??= new EventNode(id, @event);

            long expectedPosition = initialPosition + 10;
            long actualPosition = s.Offset;
            Debug.Assert(expectedPosition == actualPosition,
                $"Expected to have read {expectedPosition - initialPosition} bytes, but {actualPosition - initialPosition} have been read.");

            node.NextEventId = s.Transform(nameof(NextEventId), node.NextEventId, s.UInt16, ConvertMaxToNull.Instance);
            return node;
        }

        static IMapEvent SerdesByType(EventNode node, ISerializer s, MapEventType type) =>
            type switch // Individual parsers handle byte range [1,9]
            {
                MapEventType.Action => ActionEvent.Serdes((ActionEvent)node?.Event, s),
                MapEventType.AskSurrender => AskSurrenderEvent.Serdes((AskSurrenderEvent)node?.Event, s),
                MapEventType.ChangeIcon => ChangeIconEvent.Serdes((ChangeIconEvent)node?.Event, s),
                MapEventType.ChangeUsedItem => ChangeUsedItemEvent.Serdes((ChangeUsedItemEvent)node?.Event, s),
                MapEventType.Chest => OpenChestEvent.Serdes((OpenChestEvent)node?.Event, s),
                MapEventType.CloneAutomap => CloneAutomapEvent.Serdes((CloneAutomapEvent)node?.Event, s),
                MapEventType.CreateTransport => CreateTransportEvent.Serdes((CreateTransportEvent)node?.Event, s),
                MapEventType.DataChange => DataChangeEvent.Serdes((DataChangeEvent)node?.Event, s),
                MapEventType.DoScript => DoScriptEvent.Serdes((DoScriptEvent)node?.Event, s),
                MapEventType.Door => DoorEvent.Serdes((DoorEvent)node?.Event, s),
                MapEventType.Encounter => EncounterEvent.Serdes((EncounterEvent)node?.Event, s),
                MapEventType.EndDialogue => EndDialogueEvent.Serdes((EndDialogueEvent)node?.Event, s),
                MapEventType.Execute => ExecuteEvent.Serdes((ExecuteEvent)node?.Event, s),
                MapEventType.MapExit => TeleportEvent.Serdes((TeleportEvent)node?.Event, s),
                MapEventType.Modify => ModifyEvent.Serdes((ModifyEvent)node?.Event, s),
                MapEventType.Offset => OffsetEvent.Serdes((OffsetEvent)node?.Event, s),
                MapEventType.Pause => PauseEvent.Serdes((PauseEvent)node?.Event, s),
                MapEventType.PlaceAction => PlaceActionEvent.Serdes((PlaceActionEvent)node?.Event, s),
                MapEventType.PlayAnimation => PlayAnimationEvent.Serdes((PlayAnimationEvent)node?.Event, s),
                MapEventType.Query => QueryEvent.Serdes((QueryEvent)node?.Event, s),
                MapEventType.RemovePartyMember => RemovePartyMemberEvent.Serdes((RemovePartyMemberEvent)node?.Event, s),
                MapEventType.Script => RunScriptEvent.Serdes((RunScriptEvent)node?.Event, s),
                MapEventType.Signal => SignalEvent.Serdes((SignalEvent)node?.Event, s),
                MapEventType.SimpleChest => SimpleChestEvent.Serdes((SimpleChestEvent)node?.Event, s),
                MapEventType.Sound => SoundEvent.Serdes((SoundEvent)node?.Event, s),
                MapEventType.Spinner => SpinnerEvent.Serdes((SpinnerEvent)node?.Event, s),
                MapEventType.StartDialogue => StartDialogueEvent.Serdes((StartDialogueEvent)node?.Event, s),
                MapEventType.Text => TextEvent.Serdes((TextEvent)node?.Event, s),
                MapEventType.Trap => TrapEvent.Serdes((TrapEvent)node?.Event, s),
                MapEventType.UnkFF => DummyMapEvent.Serdes((DummyMapEvent)node?.Event, s),
                MapEventType.Wipe => WipeEvent.Serdes((WipeEvent)node?.Event, s),
                _ => null
            };
    }
}
