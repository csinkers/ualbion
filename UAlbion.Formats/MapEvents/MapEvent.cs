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
            DoScript = 0x1D
        }

        public int Id { get; set; }
        public abstract EventType Type { get; }
        public ushort? NextEventId { get; private set; }

        public static MapEvent Load(BinaryReader br, int id)
        {
            var initialPosition = br.BaseStream.Position;
            var type = (EventType)br.ReadByte(); // +0
            MapEvent result;
            switch(type)
            {
                case EventType.DataChange:        result = new DataChangeEvent(br, id); break;
                case EventType.Chest:             result = new OpenChestEvent(br, id); break;
                case EventType.MapExit:           result = new TeleportEvent(br, id); break;
                case EventType.Script:            result = new RunScriptEvent(br, id); break;
                case EventType.Door:              result = new DoorEvent(br, id); break;
                case EventType.Text:              result = new TextEvent(br, id); break;
                case EventType.Spinner:           result = new SpinnerEvent(br, id); break;
                case EventType.Trap:              result = new TrapEvent(br, id); break;
                case EventType.ChangeUsedItem:    result = new ChangeUsedItemEvent(br, id); break;
                case EventType.ChangeIcon:        result = new ChangeIconEvent(br, id); break;
                case EventType.Encounter:         result = new EncounterEvent(br, id); break;
                case EventType.PlaceAction:       result = new PlaceActionEvent(br, id); break;
                case EventType.Query:             result = new QueryEvent(br, id); break;
                case EventType.Modify:            result = new ModifyEvent(br, id); break;
                case EventType.Action:            result = new ActionEvent(br, id); break;
                case EventType.Signal:            result = new SignalEvent(br, id); break;
                case EventType.CloneAutomap:      result = new CloneAutomapEvent(br, id); break;
                case EventType.Sound:             result = new SoundEvent(br, id); break;
                case EventType.StartDialogue:     result = new StartDialogueEvent(br, id); break;
                case EventType.CreateTransport:   result = new CreateTransportEvent(br, id); break;
                case EventType.Execute:           result = new ExecuteEvent(br, id); break;
                case EventType.RemovePartyMember: result = new RemovePartyMemberEvent(br, id); break;
                case EventType.EndDialogue:       result = new EndDialogueEvent(br, id); break;
                case EventType.Wipe:              result = new WipeEvent(br, id); break;
                case EventType.PlayAnimation:     result = new PlayAnimationEvent(br, id); break;
                case EventType.Offset:            result = new OffsetEvent(br, id); break;
                case EventType.Pause:             result = new PauseEvent(br, id); break;
                case EventType.SimpleChest:       result = new SimpleChestEvent(br, id); break;
                case EventType.AskSurrender:      result = new AskSurrenderEvent(br, id); break;
                case EventType.DoScript:          result = new DoScriptEvent(br, id); break;
                default: throw new ArgumentOutOfRangeException();
            }
            Debug.Assert(br.BaseStream.Position == initialPosition + 8);
            result.NextEventId = br.ReadUInt16();
            return result;
        }
    }
}
