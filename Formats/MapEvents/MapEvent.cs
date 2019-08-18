using System;
using System.Diagnostics;
using System.IO;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    public abstract class MapEvent : IEvent
    {
        public enum EventType : byte
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

        public int Id { get; }
        public EventType Type { get; }
        public ushort? NextEventId { get; private set; }
        public MapEvent NextEvent { get; set; }

        protected MapEvent(int id, EventType type)
        {
            Id = id;
            Type = type;
        }

        public static MapEvent Load(BinaryReader br, int id)
        {
            var initialPosition = br.BaseStream.Position;
            var type = (EventType)br.ReadByte(); // +0

            MapEvent result;
            switch(type) // Individual parsers handle byte range [1,9]
            {
                case EventType.DataChange:        result = new DataChangeEvent(br, id, type); break;
                case EventType.Chest:             result = new OpenChestEvent(br, id, type); break;
                case EventType.MapExit:           result = new TeleportEvent(br, id, type); break;
                case EventType.Script:            result = new RunScriptEvent(br, id, type); break;
                case EventType.Door:              result = new DoorEvent(br, id, type); break;
                case EventType.Text:              result = new TextEvent(br, id, type); break;
                case EventType.Spinner:           result = new SpinnerEvent(br, id, type); break;
                case EventType.Trap:              result = new TrapEvent(br, id, type); break;
                case EventType.ChangeUsedItem:    result = new ChangeUsedItemEvent(br, id, type); break;
                case EventType.ChangeIcon:        result = new ChangeIconEvent(br, id, type); break;
                case EventType.Encounter:         result = new EncounterEvent(br, id, type); break;
                case EventType.PlaceAction:       result = new PlaceActionEvent(br, id, type); break;
                case EventType.Query:             result = QueryEvent.Load(br, id, type); break;
                case EventType.Modify:            result = ModifyEvent.Load(br, id, type); break;
                case EventType.Action:            result = new ActionEvent(br, id, type); break;
                case EventType.Signal:            result = new SignalEvent(br, id, type); break;
                case EventType.CloneAutomap:      result = new CloneAutomapEvent(br, id, type); break;
                case EventType.Sound:             result = new SoundEvent(br, id, type); break;
                case EventType.StartDialogue:     result = new StartDialogueEvent(br, id, type); break;
                case EventType.CreateTransport:   result = new CreateTransportEvent(br, id, type); break;
                case EventType.Execute:           result = new ExecuteEvent(br, id, type); break;
                case EventType.RemovePartyMember: result = new RemovePartyMemberEvent(br, id, type); break;
                case EventType.EndDialogue:       result = new EndDialogueEvent(br, id, type); break;
                case EventType.Wipe:              result = new WipeEvent(br, id, type); break;
                case EventType.PlayAnimation:     result = new PlayAnimationEvent(br, id, type); break;
                case EventType.Offset:            result = new OffsetEvent(br, id, type); break;
                case EventType.Pause:             result = new PauseEvent(br, id, type); break;
                case EventType.SimpleChest:       result = new SimpleChestEvent(br, id, type); break;
                case EventType.AskSurrender:      result = new AskSurrenderEvent(br, id, type); break;
                case EventType.DoScript:          result = new DoScriptEvent(br, id, type); break;
                case EventType.UnkFF:             result = new DummyMapEvent(br, id, type); break;
                default: throw new ArgumentOutOfRangeException();
            }

            long expectedPosition = initialPosition + 10;
            long actualPosition = br.BaseStream.Position;
            Debug.Assert(expectedPosition == actualPosition,
                $"Expected to have read {expectedPosition-initialPosition} bytes, but {actualPosition - initialPosition} have been read.");

            result.NextEventId = br.ReadUInt16(); // +A
            if (result.NextEventId == 0xffff) result.NextEventId = null;
            return result;
        }
    }
}
