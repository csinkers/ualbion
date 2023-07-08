﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.TestCommon;
using Xunit;

namespace UAlbion.Base.Tests;

public class EventRoundTripTests
{
    public EventRoundTripTests()
    {
        Event.AddEventsFromAssembly(typeof(ActionEvent).Assembly);
        AssetMapping.GlobalIsThreadLocal = true;
        var mapping = AssetMapping.Global;
        var disk = new MockFileSystem(true);
        var jsonUtil = new FormatJsonUtil();
        var baseDir = ConfigUtil.FindBasePath(disk);
        var typeConfigPath = Path.Combine(baseDir, "mods", "Base", "types.json");

        var tcl = new TypeConfigLoader(jsonUtil);
        var typeConfig = tcl.Load(typeConfigPath, "Base", null, mapping, disk);

        foreach (var assetType in typeConfig.IdTypes.Values)
        {
            var enumType = Type.GetType(assetType.EnumType);
            if (enumType == null)
                throw new InvalidOperationException(
                    $"Could not load enum type \"{assetType.EnumType}\" defined in \"{typeConfigPath}\"");

            mapping.RegisterAssetType(assetType.EnumType, assetType.AssetType);
        }
    }

    static void Test(string data, bool useNumeric = false)
    {
        var results = new List<(string, string)>();

        var lines = ApiUtil.SplitLines(data);
        foreach (var line in lines)
        {
            try
            {
                var e = Event.Parse(line, out _);

                var builder = new UnformattedScriptBuilder(useNumeric);
                e?.Format(builder);
                var s = builder.Build();

                if (!string.Equals(s, line, StringComparison.OrdinalIgnoreCase))
                    results.Add((line, s));
            }
            catch (Exception ex)
            {
                results.Add((line, ex.ToString()));
            }
        }

        if (results.Count > 0)
        {
            throw new FormatException(
                $"{results.Count} of {lines.Length} events failed:" +
                Environment.NewLine +
                string.Join(Environment.NewLine, results.Select((x, i) =>
                    $"{i}: \"{x.Item1}\" !=\"{x.Item2}\"")));
        }
    }

    static byte[] EventToBytes(IMapEvent e)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        using var s = new AlbionWriter(bw);
        MapEvent.SerdesEvent(e, s, AssetMapping.Global, MapType.Unknown);
        bw.Flush();
        ms.Position = 0;
        return ms.ToArray();
    }

    static IMapEvent BytesToEvent(byte[] bytes)
    {
        using var ms = new MemoryStream(bytes);
        using var br = new BinaryReader(ms);
        using var s = new AlbionReader(br);
        return MapEvent.SerdesEvent(null, s, AssetMapping.Global, MapType.Unknown);
    }

    static string Test(string scriptFormat, string expectedToStringResult, IMapEvent e)
    {
        if (!string.Equals(e.ToString(), expectedToStringResult, StringComparison.OrdinalIgnoreCase))
            return $"Event \"{e}\" did not serialise to the expected string \"{expectedToStringResult}\"";

        IMapEvent parsed;
        try
        {
            parsed = (IMapEvent)Event.Parse(scriptFormat, out var error);
            if (parsed == null)
                return $"Could not parse \"{scriptFormat}\": {error}";
        }
        catch (Exception ex) { return $"Could not parse \"{scriptFormat}\": {ex}"; }

        var bytes1 = EventToBytes(e);
        var bytes2 = EventToBytes(parsed);
        var hex1 = FormatUtil.BytesToHexString(bytes1);
        var hex2 = FormatUtil.BytesToHexString(bytes2);
        if (!string.Equals(hex1, hex2, StringComparison.Ordinal))
            return $"The literal event ({e}) serialised to {hex1}, but the parsed event ({scriptFormat}) serialised to {hex2}";

        if (scriptFormat != expectedToStringResult)
        {
            IMapEvent roundTripped;
            try
            {
                roundTripped = (IMapEvent) Event.Parse(expectedToStringResult, out var error);
                if (roundTripped == null)
                    return $"Could not parse \"{expectedToStringResult}\": {error}";
            }
            catch (Exception ex) { return $"Could not parse \"{expectedToStringResult}\": {ex}"; }

            var bytes3 = EventToBytes(roundTripped);
            var hex3 = FormatUtil.BytesToHexString(bytes3);
            if (!string.Equals(hex1, hex2, StringComparison.Ordinal))
                return $"The literal event ({e}) serialised to {hex1}, but after round-tripping through text ({expectedToStringResult}) it serialised to {hex3}";
        }

        return null;
    }

    static void Test(params (string, IMapEvent)[] events)
    {
        var results = new List<string>();

        foreach (var (line, e) in events)
        {
            var error = Test(line, line, e);
            if (error != null)
                results.Add(error);
        }

        if (results.Count > 0)
        {
            throw new FormatException(
                $"{results.Count} of {events.Length} events failed:" +
                Environment.NewLine +
                string.Join(Environment.NewLine, results));
        }
    }

    [Fact]
    public void DoorOpen()
    {
        Test(("set_door_open Set Door.Beastmaster", new SetDoorOpenEvent(SwitchOperation.Set, Base.Door.Beastmaster)));
    }

    [Fact]
    public void Action()
    {
        var unknown0 = new AssetId(AssetType.Unknown);
        var unknown1 = new AssetId(AssetType.Unknown, 1);
        Test(("action AskAboutItem 1 Item.Pistol", new ActionEvent(ActionType.AskAboutItem,   1, (ItemId)Item.Pistol, 1)),
            ("action DialogueLine 1 Unknown.1",   new ActionEvent(ActionType.DialogueLine,   1, unknown1, 1)),
            ("action EquipItem 1 Item.Pistol",    new ActionEvent(ActionType.EquipItem,      1, (ItemId)Item.Pistol, 1)),
            ("action FinishDialogue 1 Unknown.0", new ActionEvent(ActionType.FinishDialogue, 1, unknown0, 1)),
            ("action DropItem 1 Item.Pistol",     new ActionEvent(ActionType.DropItem,     1, (ItemId)Item.Pistol, 1)),
            ("action StartDialogue 1 Unknown.0",  new ActionEvent(ActionType.StartDialogue,  1, unknown0, 1)),
            ("action StartDialogue 1 Unknown.1",  new ActionEvent(ActionType.StartDialogue,  1, unknown1, 1)),
            ("action UnequipItem 1 Item.Pistol",  new ActionEvent(ActionType.UnequipItem,    1, (ItemId)Item.Pistol, 1)),
            ("action UnkE 1 Unknown.0",           new ActionEvent(ActionType.UnkE,           1, unknown0, 1)),
            ("action Unk17 1 Unknown.1",          new ActionEvent(ActionType.Unk17,          1, unknown1, 1)),
            ("action UseItem 1 Item.Pistol",      new ActionEvent(ActionType.UseItem,        1, (ItemId)Item.Pistol, 1)),
            ("action Word 1",                     new ActionEvent(ActionType.Word,           1, AssetId.None, 1)),
            ("action Word 1 Word.Argim",          new ActionEvent(ActionType.Word,           1, (WordId)Word.Argim, 1)));
    }

    [Fact]
    public void AddPartyMember()
    {
        Test(("add_party_member PartyMember.Tom", new AddPartyMemberEvent(PartyMember.Tom, NumericOperation.SetAmount, 0)));
        Test(("add_party_member PartyMember.Tom SetToMaximum", new AddPartyMemberEvent(PartyMember.Tom, NumericOperation.SetToMaximum, 0)));
    }

    [Fact]
    public void ModifyItemCount()
    {
        Test(("modify_item_count AddAmount 1 Item.Pistol", new ModifyItemCountEvent(NumericOperation.AddAmount, 1, Item.Pistol)),
            ("modify_item_count SetToMinimum 0 Item.Pistol", new ModifyItemCountEvent(NumericOperation.SetToMinimum, 0, Item.Pistol)),
            ("modify_item_count SetToMinimum 1 Item.Pistol", new ModifyItemCountEvent(NumericOperation.SetToMinimum, 1, Item.Pistol)),
            ("modify_item_count SubtractAmount 1 Item.Pistol", new ModifyItemCountEvent(NumericOperation.SubtractAmount, 1, Item.Pistol)));
    }

    [Fact]
    public void ChangeIcon()
    {
        Test(("change_icon 1 1 AbsPerm BlockHard 0 Underlay", new ChangeIconEvent(1, 1, EventScope.AbsPerm, IconChangeType.BlockHard, 0, ChangeIconLayers.Underlay, MapId.None)),
            ("change_icon 1 1 AbsPerm BlockHard 1 None", new ChangeIconEvent(1, 1, EventScope.AbsPerm, IconChangeType.BlockHard, 1, 0, MapId.None)),
            ("change_icon 1 1 RelPerm BlockHard 1", new ChangeIconEvent(1, 1, EventScope.RelPerm, IconChangeType.BlockHard, 1, ChangeIconLayers.Underlay | ChangeIconLayers.Overlay, MapId.None)),
            ("change_icon 1 1 RelTemp BlockHard 1 Overlay", new ChangeIconEvent(1, 1, EventScope.RelTemp, IconChangeType.BlockHard, 1,  ChangeIconLayers.Overlay, MapId.None)),
            ("change_icon 1 1 AbsTemp BlockHard 1 None", new ChangeIconEvent(1, 1, EventScope.AbsTemp, IconChangeType.BlockHard, 1, 0, MapId.None)),
            ("change_icon 3 25 AbsPerm Wall 108 Underlay|Overlay Map.TorontoPart22", new ChangeIconEvent(3,25,EventScope.AbsPerm,IconChangeType.Wall,108, (ChangeIconLayers)3, Map.TorontoPart22) )
        );
    }

    [Fact]
    public void ChangeNpcMovement()
    {
        Test(
            ("change_npc_movement 1 Stationary AbsPerm Underlay",
            new ChangeNpcMovementEvent(1,
                NpcMovement.Stationary,
                EventScope.AbsPerm,
                ChangeIconLayers.Underlay, MapId.None, 0)),
            ("change_npc_movement 23 RandomWander AbsPerm Underlay",
            new ChangeNpcMovementEvent(23,
                NpcMovement.RandomWander,
                EventScope.AbsPerm,
                ChangeIconLayers.Underlay, MapId.None, 0)));
    }

    [Fact]
    public void ChangeNpcSprite()
    {
        Test(
            ("change_npc_sprite 1 NpcLargeGfx.Rainer AbsPerm Underlay",
            new ChangeNpcSpriteEvent(1,
                (SpriteId)NpcLargeGfx.Rainer,
                EventScope.AbsPerm,
                ChangeIconLayers.Underlay, MapId.None)),
            ("change_npc_sprite 23 ObjectGroup.15 AbsPerm Underlay",
            new ChangeNpcSpriteEvent(23,
                new AssetId(AssetType.ObjectGroup, 15),
                EventScope.AbsPerm,
                ChangeIconLayers.Underlay, MapId.None)));
    }

    [Fact]
    public void ModifyGold()
    {
        Test(("modify_gold AddAmount 1", new ModifyGoldEvent(NumericOperation.AddAmount, 1, 0)),
            ("modify_gold SetToMinimum 0", new ModifyGoldEvent(NumericOperation.SetToMinimum, 0, 0)),
            ("modify_gold SubtractAmount 0", new ModifyGoldEvent(NumericOperation.SubtractAmount, 0, 0)),
            ("modify_gold SubtractAmount 1", new ModifyGoldEvent(NumericOperation.SubtractAmount, 1, 0)));
    }

    [Fact]
    public void ModifyRations()
    {
        Test(("modify_rations AddAmount 1", new ModifyRationsEvent(NumericOperation.AddAmount, 1, 0)),
            ("modify_rations SetToMinimum 0", new ModifyRationsEvent(NumericOperation.SetToMinimum, 0, 0)),
            ("modify_rations SubtractAmount 1", new ModifyRationsEvent(NumericOperation.SubtractAmount, 1, 0)));
    }

    [Fact]
    public void ModifyHours()
    {
        Test(("modify_hours AddAmount 1", new ModifyHoursEvent(NumericOperation.AddAmount, 1)),
            ("modify_hours SetAmount 1", new ModifyHoursEvent(NumericOperation.SetAmount, 1)));
    }

    [Fact]
    public void ChangeUsedItem()
    {
        Test(("change_used_item Item.Pistol", new ChangeUsedItemEvent(Item.Pistol)));
    }

    [Fact]
    public void CloneAutomap()
    {
        Test("clone_automap 122 164",
            "clone_automap Map.OldFormerBuilding Map.OldFormerBuildingPostFight",
            new CloneAutomapEvent(Map.OldFormerBuilding, Map.OldFormerBuildingPostFight));
    }
/*
    [Fact]
    public void ChangeUnk0()
    {
        Test(("change_unk0 AddAmount 3 7", new ChangeUnk0Event(NumericOperation.AddAmount, 3, 7, 0)),
            ("change_unk0 AddPercentage 20 4 1", new ChangeUnk0Event(NumericOperation.AddPercentage, 20, 4, 1)),
            ("change_unk0 AddPercentage 25 0 1", new ChangeUnk0Event(NumericOperation.AddPercentage, 25, 0, 1)));
    }

    [Fact]
    public void ChangeExperience()
    {
        Test(("change_experience None AddAmount 250", new ChangeExperienceEvent(PartyMemberId.None, NumericOperation.AddAmount, 250, 0)),
            ("change_experience None AddAmount 50", new ChangeExperienceEvent(PartyMemberId.None, NumericOperation.AddAmount, 50, 0)));
    }

    [Fact]
    public void ChangeFood()
    {
        Test(("change_food None AddAmount 6", new ChangeFoodEvent(PartyMemberId.None, NumericOperation.AddAmount, 6, 0)),
            ("change_food None SubtractAmount 1 1", new ChangeFoodEvent(PartyMemberId.None, NumericOperation.SubtractAmount, 1, 1)));
    }

    [Fact]
    public void ChangeGold()
    {
        Test(("change_gold None AddAmount 340", new ChangeGoldEvent(PartyMemberId.None, NumericOperation.AddAmount, 340, 0)));
    }

    [Fact]
    public void ChangeHealth()
    {
        Test(("change_health None AddAmount 2 1", new ChangeHealthEvent(PartyMemberId.None, NumericOperation.AddAmount, 2, 1, 0)),
            ("change_health None AddPercentage 25 7", new ChangeHealthEvent(PartyMemberId.None, NumericOperation.AddPercentage, 25, 7, 0)),
            ("change_health None SetToMaximum 0", new ChangeHealthEvent(PartyMemberId.None, NumericOperation.SetToMaximum, 0, 0, 0)),
            ("change_health None SubtractAmount 17 1", new ChangeHealthEvent(PartyMemberId.None, NumericOperation.SubtractAmount, 17, 1, 0)),
            ("change_health None SubtractPercentage 15 1", new ChangeHealthEvent(PartyMemberId.None, NumericOperation.SubtractPercentage, 15, 1, 0)),
            ("change_health PartyMember.Harriet AddAmount 1 2", new ChangeHealthEvent(PartyMember.Harriet, NumericOperation.AddAmount, 1, 2, 0)),
            ("change_health PartyMember.Unknown8 SetToMaximum 0 2", new ChangeHealthEvent(PartyMember.Unknown8, NumericOperation.SetToMaximum, 0, 2, 0)),
            ("change_health PartyMember.Siobhan SetToMaximum 0 2", new ChangeHealthEvent(PartyMember.Siobhan, NumericOperation.SetToMaximum, 0, 2, 0)),
            ("change_health PartyMember.Drirr SetToMaximum 0 2", new ChangeHealthEvent(PartyMember.Drirr, NumericOperation.SetToMaximum, 0, 2, 0)));
    }
*/
    [Fact]
    public void ChangeItem()
    {
        Test(("change_item Target.Leader Item.BlueHealingPotion AddAmount 1",    new ChangeItemEvent(Target.Leader,    Item.BlueHealingPotion, NumericOperation.AddAmount,      1)),
            ("change_item Target.Leader Item.Canister SetToMinimum",             new ChangeItemEvent(Target.Leader,    Item.Canister,          NumericOperation.SetToMinimum,   0)),
            ("change_item Target.Leader Item.StrengthAmulet SetToMinimum 1",     new ChangeItemEvent(Target.Leader,    Item.StrengthAmulet,    NumericOperation.SetToMinimum,   1)),
            ("change_item Target.Leader Item.BlueStaff SubtractAmount 1",        new ChangeItemEvent(Target.Leader,    Item.BlueStaff,         NumericOperation.SubtractAmount, 1)),
            ("change_item PartyMember.Sira Item.TriifalaiSeed SubtractAmount 1", new ChangeItemEvent(PartyMember.Sira, Item.TriifalaiSeed,     NumericOperation.SubtractAmount, 1)),
            ("change_item PartyMember.Tom Item.TheSeed AddAmount 1",             new ChangeItemEvent(PartyMember.Tom,  Item.TheSeed,           NumericOperation.AddAmount,      1)));
    }

    [Fact]
    public void ChangeLanguage()
    {
        Test(("change_language Target.Everyone Iskai SetToMaximum",   new ChangeLanguageEvent(Target.Everyone,    PlayerLanguage.Iskai, NumericOperation.SetToMaximum)),
            ("change_language PartyMember.Rainer Iskai SetToMaximum", new ChangeLanguageEvent(PartyMember.Rainer, PlayerLanguage.Iskai, NumericOperation.SetToMaximum)),
            ("change_language PartyMember.Tom Iskai SetToMaximum",    new ChangeLanguageEvent(PartyMember.Tom,    PlayerLanguage.Iskai, NumericOperation.SetToMaximum)));
    }
/*
    [Fact]
    public void ChangeMana()
    {
        Test(("change_mana None AddPercentage 20 7", new ChangeManaEvent(PartyMemberId.None, NumericOperation.AddPercentage, 20, 7)),
            ("change_mana None AddPercentage 50 1", new ChangeManaEvent(PartyMemberId.None, NumericOperation.AddPercentage, 50, 1)),
            ("change_mana None SetToMaximum 0", new ChangeManaEvent(PartyMemberId.None, NumericOperation.SetToMaximum, 0, 0)),
            ("change_mana None SetToMaximum 0 1", new ChangeManaEvent(PartyMemberId.None, NumericOperation.SetToMaximum, 0, 1)));
    }
*/
    [Fact]
    public void ChangeStatus()
    {
        Test(("change_status Target.Leader Intoxicated SetToMaximum",    new ChangeStatusEvent(Target.Leader,   PlayerCondition.Intoxicated, NumericOperation.SetToMaximum)),
            ("change_status Target.Leader Irritated SetToMaximum",       new ChangeStatusEvent(Target.Leader,   PlayerCondition.Irritated,   NumericOperation.SetToMaximum)),
            ("change_status Target.Everyone Unconscious SetToMaximum 3", new ChangeStatusEvent(Target.Everyone, PlayerCondition.Unconscious, NumericOperation.SetToMaximum,  3)),
            ("change_status Target.Subject Poisoned SetToMinimum",       new ChangeStatusEvent(Target.Subject, PlayerCondition.Poisoned, NumericOperation.SetToMinimum)),
            ("change_status Target.Leader Unconscious SetToMinimum 10",  new ChangeStatusEvent(Target.Leader,   PlayerCondition.Unconscious, NumericOperation.SetToMinimum, 10)),
            ("change_status Target.Everyone Unconscious SetToMinimum 3", new ChangeStatusEvent(Target.Everyone, PlayerCondition.Unconscious, NumericOperation.SetToMinimum,  3)));
    }
/*
    [Fact]
    public void ChangeUnkB()
    {
        Test(("change_unkb 115 SetAmount 116 0 5", new ChangeUnkBEvent(115, NumericOperation.SetAmount, 116, 0, 5)),
            ("change_unkb 220 SetAmount 220 228 5", new ChangeUnkBEvent(220, NumericOperation.SetAmount, 220, 228, 5)),
            ("change_unkb 237 SetAmount 255 0 5", new ChangeUnkBEvent(237, NumericOperation.SetAmount, 255, 0, 5)));
    }
*/
    [Fact]
    public void ChainOff()
    {
        Test(@"chain_off Clear 1
chain_off Set 1
chain_off Set 1 Map.Jirinaar");
    }

    [Fact]
    public void DoScript()
    {
        Test(@"do_script Script.TomMeetsChristine_300");
    }

    [Fact]
    public void EncounterEvent()
    {
        Test(@"encounter 1 2");
    }

    [Fact]
    public void EndDialogue()
    {
        Test(@"end_dialogue");
    }

    [Fact]
    public void Execute()
    {
        Test(@"execute 0 1
execute 1 1");
    }

    [Fact]
    public void Inv()
    {
        Test(@"chest Chest.HClan_3Stim25r Item.Pistol 1 2 3
chest Chest.HClan_3Stim25r
door Door.HerrasDoor_HerrasKey Item.HerrasKey 1 2 3
door Door.HerrasDoor_HerrasKey");
    }

    [Fact]
    public void Pause()
    {
        Test(@"pause 1");
    }

    [Fact]
    public void PlaceAction()
    {
        Test(@"place_action AskOpinion 0 0 0 0 1 1
place_action AskOpinion 1 1 1 1 1 1
place_action Cure 1 1 1 1 1 1
place_action Heal 1 1 1 1 1 1
place_action LearnCloseCombat 1 1 1 0 1 0
place_action LearnCloseCombat 1 1 1 1 1 0
place_action LearnSpells 1 1 1 1 1 1
place_action Merchant 1 1 1 0 1 1
place_action Merchant 1 1 1 1 1 1
place_action OrderFood 1 1 1 0 1 0
place_action OrderFood 1 1 1 0 1 1
place_action OrderFood 1 1 1 1 1 0
place_action OrderFood 1 1 1 1 1 1
place_action RemoveCurse 1 1 1 0 0 0
place_action RemoveCurse 1 1 1 0 0 1
place_action RemoveCurse 1 1 1 0 1 0
place_action RemoveCurse 1 1 1 1 1 1
place_action RepairItem 1 1 1 0 1 0
place_action RepairItem 1 1 1 0 1 1
place_action RepairItem 1 1 1 1 1 0
place_action RestoreItemEnergy 1 1 1 0 1 0
place_action ScrollMerchant 1 1 1 0 1 1
place_action SleepInRoom 1 1 1 1 1 0
place_action SleepInRoom 1 1 1 1 1 1");
    }

    [Fact]
    public void PlayAnim()
    {
        Test((@"play_anim Video.MagicDemonstration 1 2 3 4", new PlayAnimationEvent(Video.MagicDemonstration, 1, 2, 3, 4, 0, 0)));
    }

    [Fact] public void FestivalQuery() => Test(@"prompt_player 1 NonZero 1");

    [Fact]
    public void Query()
    {
        Test(@"prompt_player 1
prompt_player 1 NonZero 1
prompt_player_numeric Equals 0 1
is_conscious PartyMember.Tom
is_conscious PartyMember.Tom NonZero 1
is_demo_version 1 NonZero 0
event_used
total_gold GreaterThan 0 0
total_gold GreaterThanOrEqual 0 1
total_gold GreaterThanOrEqual 1 0
total_gold LessThanOrEqual 0 1
has_item Equals 1 Item.Pistol
has_item GreaterThan 0 Item.Pistol
has_item GreaterThanOrEqual 1 Item.Pistol
in_party PartyMember.Tom
is_leader PartyMember.Tom Equals
is_leader PartyMember.Tom
current_map Equals 0 Map.Unk1
is_npc_active 0
is_npc_active 0 Equals
is_npc_active 0 NonZero 1
is_npc_active 1
is_npc_active 1 Equals 1
result
random_chance 1 NonZero 1
random_chance 1 LessThanOrEqual 0
random_chance 1 LessThanOrEqual 1
random_chance 1 LessThan 1
random_chance 1 AlwaysFalse2 0
is_debug_mode
get_switch Switch.ExpelledFromSouthWind
get_ticker Ticker.Ticker100 Equals 1
get_ticker Ticker.Ticker100 GreaterThan 1
get_ticker Ticker.Ticker100 GreaterThanOrEqual 1
get_ticker Ticker.Ticker100 LessThanOrEqual 1
get_ticker Ticker.Ticker100 LessThan 1
query_hour Equals 1 1
is_chain_active 1 Map.TorontoBegin Equals
is_chain_active 1 Map.TorontoBegin GreaterThan
is_chain_active 1 Map.TorontoBegin GreaterThan
is_chain_active 1 Map.TorontoBegin GreaterThanOrEqual
is_chain_active 1 Map.TorontoBegin GreaterThanOrEqual
is_chain_active 0 None NonZero
is_chain_active 1 None NonZero
is_chain_active 1 Map.TorontoBegin LessThanOrEqual
is_chain_active 1 Map.TorontoBegin LessThanOrEqual
is_chain_active 1 Map.TorontoBegin LessThan
is_chain_active 1 Map.TorontoBegin AlwaysFalse2
query_unk1e GreaterThan 0 1
query_unk1e GreaterThanOrEqual 0 1
query_unk1e LessThanOrEqual 0 1
query_unk1e AlwaysFalse2 0 1
query_unkc Equals 0 0
query_unkc Equals 0 1
used_item Item.Pistol Equals
used_item Item.Pistol
verb MapInit");
    }

    [Fact]
    public void RemovePartyMember()
    {
        Test(@"remove_party_member None 1 1
remove_party_member PartyMember.Tom 1 1");
    }

    [Fact]
    public void MapLighting()
    {
        Test(@"map_lighting NeedTorch");
        Test(@"map_lighting NeedTorch 1");
    }

    [Fact]
    public void DisableNpc()
    {
        Test(@"modify_npc_off Set 0
modify_npc_off Set 0
modify_npc_off Set 1 Map.Jirinaar");
    }

    [Fact]
    public void SetPartyLeader()
    {
        Test(@"set_party_leader PartyMember.Tom
set_party_leader PartyMember.Tom 1 1");
    }

    [Fact]
    public void SetTemporarySwitch()
    {
        Test(@"switch Clear Switch.ExpelledFromSouthWind
switch Clear Switch.ExpelledFromSouthWind 1
switch Set Switch.ExpelledFromSouthWind
switch Set Switch.ExpelledFromSouthWind 1
switch Toggle Switch.ExpelledFromSouthWind");
    }

    [Fact]
    public void Ticker()
    {
        Test(@"ticker Ticker.Ticker100 AddAmount 1
ticker Ticker.Ticker100 SetAmount 1
ticker Ticker.Ticker100 SetToMinimum 1
ticker Ticker.Ticker100 SubtractAmount 1");
    }

    [Fact]
    public void Signal()
    {
        Test(@"signal 1");
    }

    [Fact]
    public void SimpleChest()
    {
        Test(@"simple_chest 1 Gold.0
simple_chest 1 Item.Pistol
simple_chest 1 None
simple_chest 1 Rations.0");
    }

    [Fact]
    public void Sound()
    {
        Test(@"sound None 0 0 0 0
sound None 1 1 1 11000
sound Sample.AmbientThrum 0 0 0 0 Silent
sound Sample.IllTemperedLlama 0 1 0 0
sound Sample.IllTemperedLlama 0 1 1 0
sound Sample.IllTemperedLlama 0 1 1 11000
sound Sample.IllTemperedLlama 1 1 1 0
sound Sample.IllTemperedLlama 1 1 0 11000
sound Sample.IllTemperedLlama 1 1 1 11000");
    }

    [Fact]
    public void Spinner()
    {
        Test(@"spinner 1");
    }

    [Fact]
    public void StartDialogue()
    {
        Test(@"start_dialogue NpcSheet.Christine");
    }

    [Fact]
    public void Teleport()
    {
        Test(@"teleport Map.Jirinaar 1 1 North 1 0
teleport Map.Jirinaar 1 1 North 1 1
teleport None 1 1 North 1 1");
    }

    [Fact]
    public void Text()
    {
        Test(@"text 1
text 1 1
text 1 Conversation
text 1 ConversationOptions
text 1 ConversationQuery
text 1 NoPortrait NpcSheet.Christine
text 1 PortraitLeft PartySheet.Rainer
text 1 PortraitLeft2
text 1 StandardOptions
text 1 PortraitLeft
text 1 PortraitLeft2
text 1 PortraitLeft3 NpcSheet.Christine
text 1 QuickInfo");
    }

    [Fact]
    public void Trap()
    {
        Test(@"trap 1 1 0 0 0
trap 1 1 0 0 1
trap 1 1 1 0 1
trap 1 1 1 1 0
trap 1 1 1 1 1");
    }

    [Fact]
    public void Wipe()
    {
        Test(@"wipe 0
wipe 1");
    }

    [Fact]
    public void TestTrailingNullEvent()
    {
        Test("camera_jump 300 300", true);
    }

    [Fact]
    public void TestScriptEvents()
    {
        Test(
            @"active_member_text 100
ambient Song.JungleTownAmbient2
camera_jump 300 300
camera_lock
camera_move -4 4
camera_unlock
clear_quest_bit Switch.Switch236
do_event_chain 1
fade_from_black
fade_from_white
fade_to_black
fade_to_white
Fill_screen 0
fill_screen 194
fill_screen_0
load_pal Palette.FirstIslandDay
npc_jump 1 123 213
npc_lock 12
npc_move 16 -1 1
npc_off 11
npc_on 33
npc_text NpcSheet.Khunagh 158
npc_turn 11 West
npc_unlock 1
party_jump 170 63
party_member_text PartyMember.Tom 112
party_move -1 1
party_off
party_on
party_turn West
pause 120
play 12
play_anim Video.ThrowingSeed 140 20 1 1
show_map
show_pic Picture.CelticIskaiStandoff
show_pic Picture.TorontoWithVines 0 0
show_picture Picture.HarbourTown 0 0
song Song.TechCombatMusic
sound Sample.DistantCollapse 80 0 0 0
sound Sample.DistantCollapse 80 0 0 7000
sound Sample.DistantCollapse 80 150 0 0
sound Sample.XylophoneTones 90 100 0 22000
sound Sample.Healing 80 0 0 0
sound_fx_off
start_anim 10 0 0 1
start_anim 3 25 24 2
stop_anim
text 101
update 1
update 200");
    }

    [Fact]
    public void TestScriptEventsNumeric()
    {
        Test(
@"active_member_text 100
ambient 32
camera_jump 300 300
camera_lock
camera_move -4 4
camera_unlock
clear_quest_bit 236
do_event_chain 1
fade_from_black
fade_from_white
fade_to_black
fade_to_white
Fill_screen 0
fill_screen 194
fill_screen_0
load_pal 1
npc_jump 1 123 213
npc_lock 12
npc_move 16 -1 1
npc_off 11
npc_on 33
npc_text 1 158
npc_turn 11 3
npc_unlock 1
party_jump 170 63
party_member_text 1 112
party_move -1 1
party_off
party_on
party_turn 3
pause 120
play 12
play_anim 14 140 20 1 1
show_map
show_pic 10
show_pic 15 0 0
show_picture 2 0 0
song 20
sound 36 80 0 0 0
sound 36 80 0 0 7000
sound 36 80 150 0 0
sound_effect 34 90 100 0 22000
sound_effect 38 80 0 0 0
sound_fx_off
start_anim 10 0 0 1
start_anim 3 25 24 2
stop_anim
text 101
update 1
update 200", true);
    }
}
