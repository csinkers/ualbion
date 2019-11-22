using System;
using System.Diagnostics;
using System.IO;
using UAlbion.Api;

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
        IEvent Event { get; }
        IEventNode NextEvent { get; }
    }

    public interface IBranchNode : IEventNode
    {
        IEventNode NextEventWhenFalse { get; }
    }

    public class BranchNode : EventNode, IBranchNode
    {
        public BranchNode(int id, IEvent @event, ushort? falseEventId) : base(id, @event)
        {
            NextEventWhenFalseId = falseEventId;
        }
        public IEventNode NextEventWhenFalse { get; set; }
        public ushort? NextEventWhenFalseId { get; private set; }
    }

    public class EventNode : IEventNode
    {
        public int Id { get; private set; }
        public IEvent Event { get; private set; }
        public ushort? NextEventId { get; private set; }
        public IEventNode NextEvent { get; set; }

        public EventNode(int id, IEvent @event)
        {
            Id = id;
            Event = @event;
        }

        public static IEventNode Load(BinaryReader br, int id)
        {
            var initialPosition = br.BaseStream.Position;
            var type = (MapEventType)br.ReadByte(); // +0

            EventNode result;
            switch (type) // Individual parsers handle byte range [1,9]
            {
                case MapEventType.Action: result = ActionEvent.Load(br, id, type); break;
                case MapEventType.AskSurrender: result = AskSurrenderEvent.Load(br, id, type); break;
                case MapEventType.ChangeIcon: result = ChangeIconEvent.Load(br, id, type); break;
                case MapEventType.ChangeUsedItem: result = ChangeUsedItemEvent.Load(br, id, type); break;
                case MapEventType.Chest: result = OpenChestEvent.Load(br, id, type); break;
                case MapEventType.CloneAutomap: result = CloneAutomapEvent.Load(br, id, type); break;
                case MapEventType.CreateTransport: result = CreateTransportEvent.Load(br, id, type); break;
                case MapEventType.DataChange: result = DataChangeEvent.Load(br, id, type); break;
                case MapEventType.DoScript: result = DoScriptEvent.Load(br, id); break;
                case MapEventType.Door: result = DoorEvent.Load(br, id, type); break;
                case MapEventType.Encounter: result = EncounterEvent.Load(br, id, type); break;
                case MapEventType.EndDialogue: result = EndDialogueEvent.Load(br, id, type); break;
                case MapEventType.Execute: result = ExecuteEvent.Load(br, id, type); break;
                case MapEventType.MapExit: result = TeleportEvent.Load(br, id, type); break;
                case MapEventType.Modify: result = ModifyEvent.Load(br, id, type); break;
                case MapEventType.Offset: result = OffsetEvent.Load(br, id, type); break;
                case MapEventType.Pause: result = PauseEvent.Load(br, id, type); break;
                case MapEventType.PlaceAction: result = PlaceActionEvent.Load(br, id, type); break;
                case MapEventType.PlayAnimation: result = PlayAnimationEvent.Load(br, id, type); break;
                case MapEventType.Query: result = QueryEvent.Load(br, id, type); break;
                case MapEventType.RemovePartyMember: result = RemovePartyMemberEvent.Load(br, id, type); break;
                case MapEventType.Script: result = RunScriptEvent.Load(br, id, type); break;
                case MapEventType.Signal: result = SignalEvent.Load(br, id, type); break;
                case MapEventType.SimpleChest: result = SimpleChestEvent.Load(br, id, type); break;
                case MapEventType.Sound: result = SoundEvent.Load(br, id, type); break;
                case MapEventType.Spinner: result = SpinnerEvent.Load(br, id, type); break;
                case MapEventType.StartDialogue: result = StartDialogueEvent.Load(br, id, type); break;
                case MapEventType.Text: result = TextEvent.Load(br, id, type); break;
                case MapEventType.Trap: result = TrapEvent.Load(br, id, type); break;
                case MapEventType.UnkFF: result = DummyMapEvent.Load(br, id, type); break;
                case MapEventType.Wipe: result = WipeEvent.Load(br, id, type); break;
                default: throw new ArgumentOutOfRangeException();
            }

            long expectedPosition = initialPosition + 10;
            long actualPosition = br.BaseStream.Position;
            Debug.Assert(expectedPosition == actualPosition,
                $"Expected to have read {expectedPosition - initialPosition} bytes, but {actualPosition - initialPosition} have been read.");

            result.NextEventId = br.ReadUInt16(); // +A
            if (result.NextEventId == 0xffff) result.NextEventId = null;

            return result;
        }
    }
}
