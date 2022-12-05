using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;

namespace UAlbion.Formats.MapEvents;

public abstract class MapEvent : Event, IMapEvent
{
    public abstract MapEventType EventType { get; }

    public static EventNode SerdesNode(ushort id, EventNode node, ISerializer s, AssetMapping mapping)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        var initialPosition = s.Offset;
        var mapEvent = node?.Event as MapEvent;
        if (node?.Event != null && mapEvent == null)
            throw new ArgumentOutOfRangeException($"Tried to serialise a non-map event \"{node.Event}\" to bytes");

        var @event = SerdesEvent(mapEvent, s, mapping);

        if (@event is IBranchingEvent be)
        {
            node ??= new BranchNode(id, be);
            var branch = (BranchNode)node;
            ushort? falseEventId = s.Transform<ushort, ushort?>(
                nameof(branch.NextIfFalse),
                branch.NextIfFalse?.Id,
                S.UInt16,
                MaxToNullConverter.Instance);

            if (falseEventId != null && branch.NextIfFalse == null)
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

    public static IMapEvent SerdesEvent(IMapEvent e, ISerializer s, AssetMapping mapping)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        var initialPosition = s.Offset;
        s.Begin();
        var type = s.EnumU8("Type", e?.EventType ?? MapEventType.UnkFf);
        e = type switch // Individual parsers handle byte range [1,9]
        {
            MapEventType.Action => ActionEvent.Serdes((ActionEvent)e, mapping, s),
            MapEventType.AskSurrender => AskSurrenderEvent.Serdes((AskSurrenderEvent)e, s),
            MapEventType.ChangeIcon => ChangeIconEvent.Serdes((ChangeIconEvent)e, mapping, s),
            MapEventType.ChangeUsedItem => ChangeUsedItemEvent.Serdes((ChangeUsedItemEvent)e, mapping, s),
            MapEventType.Chest => ChestEvent.Serdes((ChestEvent)e, mapping, s),
            MapEventType.CloneAutomap => CloneAutomapEvent.Serdes((CloneAutomapEvent)e, mapping, s),
            MapEventType.CreateTransport => CreateTransportEvent.Serdes((CreateTransportEvent)e, s),
            MapEventType.DataChange => DataChangeEvent.Serdes((IDataChangeEvent)e, mapping, s),
            MapEventType.Door => DoorEvent.Serdes((DoorEvent)e, mapping, s),
            MapEventType.Encounter => EncounterEvent.Serdes((EncounterEvent)e, s),
            MapEventType.EndDialogue => EndDialogueEvent.Serdes((EndDialogueEvent)e, s),
            MapEventType.Execute => ExecuteEvent.Serdes((ExecuteEvent)e, s),
            MapEventType.MapExit => TeleportEvent.Serdes((TeleportEvent)e, mapping, s),
            MapEventType.Modify => ModifyEvent.BaseSerdes((ModifyEvent)e, mapping, s),
            MapEventType.Offset => OffsetEvent.Serdes((OffsetEvent)e, s),
            MapEventType.Pause => PauseEvent.Serdes((PauseEvent)e, s),
            MapEventType.PlaceAction => PlaceActionEvent.Serdes((PlaceActionEvent)e, s),
            MapEventType.PlayAnimation => PlayAnimationEvent.Serdes((PlayAnimationEvent)e, mapping, s),
            MapEventType.Query => QueryEvent.Serdes((QueryEvent)e, mapping, s),
            MapEventType.RemovePartyMember => RemovePartyMemberEvent.Serdes((RemovePartyMemberEvent)e, mapping, s),
            MapEventType.Script => DoScriptEvent.Serdes((DoScriptEvent)e, mapping, s),
            MapEventType.Signal => SignalEvent.Serdes((SignalEvent)e, s),
            MapEventType.SimpleChest => SimpleChestEvent.Serdes((SimpleChestEvent)e, mapping, s),
            MapEventType.Sound => SoundEvent.Serdes((SoundEvent)e, mapping, s),
            MapEventType.Spinner => SpinnerEvent.Serdes((SpinnerEvent)e, s),
            MapEventType.StartDialogue => StartDialogueEvent.Serdes((StartDialogueEvent)e, mapping, s),
            MapEventType.Text => TextEvent.Serdes((TextEvent)e, mapping, s),
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

/* ==  Binary Serialisable Event types: ==
 1 Teleport (teleport 300 32 75)
 2 Door     (open_door ...)
 3 Chest    (open_chest ...)
 4 Text     (map_text 100)
 5 Spinner  (spinner ...)
 6 Trap     (trap ...)
 7 ChangeUsedItem (change_used_item ...)
 8 DataChange (further subdivided by operation: min,max,?,set,add,sub,add%,sub%)
     0 Unk0 (TODO)
     2 Health     (party[Tom].health += 20%)
     3 Mana       (party[Sira].mana -= 5)
     5 Status     (party[Rainer].status[Poisoned] = max)
     7 Language   (party[Tom].language[Iskai] = max)
     8 Experience (party[Drirr].experience += 2000)
     B UnkB (TODO)
     C UnkC (TODO)
    13 Item (party[Tom].items[LughsShield] = 1)
    14 Gold (party[Joe].gold += 12)
    15 Food (party[Siobhan].food -= 10%)
 9 ChangeIcon (scope: rel vs abs, temp vs perm)
    0 Underlay    (map.tempUnderlay[23,12] = 47)
    1 Overlay     (map.permOverlay[+0,-3] = 1231)
    2 Wall        (map.tempWall[10, 10] = 7)
    3 Floor       (map.permFloor[64, 64] = 1)
    4 Ceiling     (map.permCeiling[12, 24] = 7)
    5 NpcMovement (npc[12].permMovement = 3)
    6 NpcSprite   (npc[5].tempSprite = 14)
    7 Chain       (map.tempChain[10, 10] = 15)
    8 BlockHard   (block_hard ...)
    9 BlockSoft   (block_soft ...)
    A Trigger     (map.tempTrigger[96, 7] = Normal)
 A Encounter (encounter ...)
 B PlaceAction
     0 LearnCloseCombat
     1 Heal
     2 Cure
     3 RemoveCurse
     4 AskOpinion
     5 RestoreItemEnergy
     6 SleepInRoom
     7 Merchant
     8 OrderFood
     9 ScrollMerchant
     B LearnSpells
     C RepairItem
 C Query (further subdivided by operation: NZ, <=, !=, ==, >=, >, <)
     0 Switch (switch[100]), (!switch[203]), (switch[KhunagMentionedSecretPassage])
     1 Unk1
     4 Unk4
     5 HasPartyMember (party[Tom].isPresent)
     6 HasItem        (!party.hasItem[Axe])
     7 UsedItem       (context.usedItem == Pick)
     9 PreviousActionResult (context.result)
     A ScriptDebugMode      (context.isDebug)
     C UnkC
     E NpcActive      (npc[16].isActive)
     F Gold           (party.gold > 100)
    11 RandomChance   (random(50))
    12 Unk12
    14 ChosenVerb     (context.verb == Examine)
    15 Conscious      (party[Tom].isConscious)
    1A Leader         (party[Rainer].isLeader)
    1C Ticker         (ticker[50] > 12)
    1D Map            (context.map == Drinno3)
    1E Unk1E
    1F PromptPlayer   (askYesNo(100))
    19 Unk19
    20 TriggerType    (context.trigger == UseItem)
    21 Unk21
    22 EventUsed      (context.event[108].hasRun)
    23 DemoVersion    (context.isDemo)
    29 Unk29
    2A Unk2A
    2B PromptPlayerNumeric (askNumeric() = 1042)
 D Modify
     0 Switch            (switch[125] = 1)
     1 DisableEventChain (map[CantosHouse].chain[120] = 0)
     2 Unk2
     4 NpcActive         (set_npc_active ...)
     5 AddPartyMember    (add_party_member ...)
     6 InventoryItem     (party.item[Knife] += 3)
     B Lighting          (map.lighting = 5) ??
     F PartyGold         (party.gold = min)
    10 PartyRations      (party.rations += 12)
    12 Time              (context.time += 6)
    1A Leader            (party.leader = Tom)
    1C Ticker            (ticker[93] = 108)  
 E Action (action ...)
     0 Word
     1 AskAboutItem
     2 Unk2 // Pay money? See ES156 (Garris, Gratogel sailor)
     4 Unk4
     5 Unk5
     6 StartDialogue
     7 FinishDialogue
     8 DialogueLine
     9 Unk9
     E Unk14
    17 Unk23
    2D Unk45
    2E UseItem
    2F EquipItem
    30 UnequipItem
    36 PickupItem
    39 Unk57
    3D Unk61
 F Signal            (signal ...)
10 CloneAutomap      (clone_automap ...)
11 Sound             (sound ...)
12 StartDialogue     (start_dialogue ...)
13 CreateTransport   (???)
14 Execute           (execute)
15 RemovePartyMember (remove_party_member ...)
16 EndDialogue       (end_dialogue)
17 Wipe              (wipe ...)
18 PlayAnimation     (play_anim ...)
19 Offset            (offset 0 0)
1A Pause             (pause 3)
1B SimpleChest       (simple_chest ...)
1C AskSurrender      (ask_surrender)
1D Script            (script 15)
FF UnkFF
*/
}