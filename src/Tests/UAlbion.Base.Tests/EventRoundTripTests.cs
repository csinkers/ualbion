using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using Xunit;

namespace UAlbion.Base.Tests
{
    public class EventRoundTripTests
    {
        static readonly string BaseDir = ConfigUtil.FindBasePath();
        public EventRoundTripTests()
        {
            AssetMapping.GlobalIsThreadLocal = true;
            var mapping = AssetMapping.Global;
            var assetConfigPath = Path.Combine(BaseDir, "mods", "Base", "assets.json");
            var assetConfig = AssetConfig.Load(assetConfigPath);

            foreach (var assetType in assetConfig.IdTypes.Values)
            {
                var enumType = Type.GetType(assetType.EnumType);
                if (enumType == null)
                    throw new InvalidOperationException(
                            $"Could not load enum type \"{assetType.EnumType}\" defined in \"{assetConfigPath}\"");

                mapping.RegisterAssetType(assetType.EnumType, assetType.AssetType);
            }
        }

        static void Test(string data, bool useNumeric = false)
        {
            var results = new List<(string, string)>();

            var lines = FormatUtil.SplitLines(data);
            foreach (var line in lines)
            {
                try
                {
                    var e = Event.Parse(line);
                    var s = e?.ToString();
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
            MapEvent.Serdes(e, s, EventSetId.None, TextId.None, AssetMapping.Global);
            bw.Flush();
            ms.Position = 0;
            return ms.ToArray();
        }

        static IMapEvent BytesToEvent(byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            using var br = new BinaryReader(ms);
            using var s = new AlbionReader(br);
            return MapEvent.Serdes(null, s, EventSetId.None, TextId.None, AssetMapping.Global);
        }

        static string Test(string scriptFormat, string expectedToStringResult, IMapEvent e)
        {
            if (!string.Equals(e.ToString(), expectedToStringResult, StringComparison.OrdinalIgnoreCase))
                return $"Event \"{e}\" did not serialise to the expected string \"{expectedToStringResult}\"";

            IMapEvent parsed;
            try { parsed = (IMapEvent)Event.Parse(scriptFormat); }
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
                try { roundTripped = (IMapEvent) Event.Parse(expectedToStringResult); }
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
        public void DummyModify()
        {
            Test(("modify_unk2 1 2 3 4 5 6", new ModifyUnk2Event(1, 2, 3, 4, 5, 6)));
        }

        [Fact]
        public void Action()
        {
            Test(("action AskAboutItem 1 Item.Pistol 1", new ActionEvent(ActionType.AskAboutItem,   1, (ItemId)Item.Pistol, 1)),
                ("action DialogueLine 1 Unknown.1 1",   new ActionEvent(ActionType.DialogueLine,   1, new AssetId(AssetType.Unknown, 1), 1)),
                ("action EquipItem 1 Item.Pistol 1",    new ActionEvent(ActionType.EquipItem,      1, (ItemId)Item.Pistol, 1)),
                ("action FinishDialogue 1 Unknown.0 1", new ActionEvent(ActionType.FinishDialogue, 1, new AssetId(AssetType.Unknown), 1)),
                ("action PickupItem 1 Item.Pistol 1",   new ActionEvent(ActionType.PickupItem,     1, (ItemId)Item.Pistol, 1)),
                ("action StartDialogue 1 Unknown.0 1",  new ActionEvent(ActionType.StartDialogue,  1, new AssetId(AssetType.Unknown), 1)),
                ("action StartDialogue 1 Unknown.1 1",  new ActionEvent(ActionType.StartDialogue,  1, new AssetId(AssetType.Unknown, 1), 1)),
                ("action UnequipItem 1 Item.Pistol 1",  new ActionEvent(ActionType.UnequipItem,    1, (ItemId)Item.Pistol, 1)),
                ("action Unk14 1 Unknown.0 1",          new ActionEvent(ActionType.Unk14,          1, new AssetId(AssetType.Unknown), 1)),
                ("action Unk23 1 Unknown.1 1",          new ActionEvent(ActionType.Unk23,          1, new AssetId(AssetType.Unknown, 1), 1)),
                ("action UseItem 1 Item.Pistol 1",      new ActionEvent(ActionType.UseItem,        1, (ItemId)Item.Pistol, 1)),
                ("action Word 1 None 1",              new ActionEvent(ActionType.Word,           1, AssetId.None, 1)),
                ("action Word 1 Word.Argim 1",          new ActionEvent(ActionType.Word,           1, (WordId)Word.Argim, 1)));
        }

        [Fact]
        public void AddPartyMember()
        {
            Test(("add_party_member PartyMember.Tom 1 0", new AddPartyMemberEvent(PartyMember.Tom, 1, 0)),
                ("add_party_member PartyMember.Tom 1 1", new AddPartyMemberEvent(PartyMember.Tom, 1, 1)));
        }

        [Fact]
        public void AddRemoveInvItem()
        {
            Test(("add_remove_inv AddAmount 1 Item.Pistol", new AddRemoveInventoryItemEvent(NumericOperation.AddAmount, 1, Item.Pistol)),
                ("add_remove_inv SetToMinimum 0 Item.Pistol", new AddRemoveInventoryItemEvent(NumericOperation.SetToMinimum, 0, Item.Pistol)),
                ("add_remove_inv SetToMinimum 1 Item.Pistol", new AddRemoveInventoryItemEvent(NumericOperation.SetToMinimum, 1, Item.Pistol)),
                ("add_remove_inv SubtractAmount 1 Item.Pistol", new AddRemoveInventoryItemEvent(NumericOperation.SubtractAmount, 1, Item.Pistol)));
        }

        [Fact]
        public void ChangeIcon()
        {
            Test(("change_icon 1 1 0 BlockHard 0 1", new ChangeIconEvent(1, 1, 0, IconChangeType.BlockHard, 0, 1)),
                ("change_icon 1 1 0 BlockHard 1 0", new ChangeIconEvent(1, 1, 0, IconChangeType.BlockHard, 1, 0)),
                ("change_icon 1 1 Rel BlockHard 1 3", new ChangeIconEvent(1, 1, EventScopes.Rel, IconChangeType.BlockHard, 1, 3)),
                ("change_icon 1 1 Rel|Temp BlockHard 1 2", new ChangeIconEvent(1, 1, EventScopes.Rel | EventScopes.Temp, IconChangeType.BlockHard, 1,  2)),
                ("change_icon 1 1 Temp BlockHard 1 0", new ChangeIconEvent(1, 1, EventScopes.Temp, IconChangeType.BlockHard, 1, 0)));
        }

        [Fact]
        public void ChangePartyGold()
        {
            Test(("change_party_gold AddAmount 1 0", new ChangePartyGoldEvent(NumericOperation.AddAmount, 1, 0)),
                ("change_party_gold AddAmount 1 1", new ChangePartyGoldEvent(NumericOperation.AddAmount, 1, 1)),
                ("change_party_gold SetToMinimum 0 0", new ChangePartyGoldEvent(NumericOperation.SetToMinimum, 0, 0)),
                ("change_party_gold SubtractAmount 0 1", new ChangePartyGoldEvent(NumericOperation.SubtractAmount, 0, 1)),
                ("change_party_gold SubtractAmount 1 0", new ChangePartyGoldEvent(NumericOperation.SubtractAmount, 1, 0)),
                ("change_party_gold SubtractAmount 1 1", new ChangePartyGoldEvent(NumericOperation.SubtractAmount, 1, 1)));
        }

        [Fact]
        public void ChangePartyRations()
        {
            Test(("change_party_rations AddAmount 1 0", new ChangePartyRationsEvent(NumericOperation.AddAmount, 1, 0)),
                ("change_party_rations AddAmount 1 1", new ChangePartyRationsEvent(NumericOperation.AddAmount, 1, 1)),
                ("change_party_rations SetToMinimum 0 0", new ChangePartyRationsEvent(NumericOperation.SetToMinimum, 0, 0)),
                ("change_party_rations SubtractAmount 0 1", new ChangePartyRationsEvent(NumericOperation.SubtractAmount, 0, 1)),
                ("change_party_rations SubtractAmount 1 0", new ChangePartyRationsEvent(NumericOperation.SubtractAmount, 1, 0)),
                ("change_party_rations SubtractAmount 1 1", new ChangePartyRationsEvent(NumericOperation.SubtractAmount, 1, 1)));
        }

        [Fact]
        public void ChangeTime()
        {
            Test(("change_time AddAmount 1", new ChangeTimeEvent(NumericOperation.AddAmount, 1)),
                ("change_time SetAmount 1", new ChangeTimeEvent(NumericOperation.SetAmount, 1)));
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

        [Fact]
        public void ChangeUnk0()
        {
            Test(("change_unk0 AddAmount 3 7 0", new ChangeUnk0Event(NumericOperation.AddAmount, 3, 7, 0)),
                ("change_unk0 AddPercentage 20 4 1", new ChangeUnk0Event(NumericOperation.AddPercentage, 20, 4, 1)),
                ("change_unk0 AddPercentage 25 0 1", new ChangeUnk0Event(NumericOperation.AddPercentage, 25, 0, 1)));
        }

        [Fact]
        public void ChangeExperience()
        {
            Test(("change_experience None AddAmount 250 0", new ChangeExperienceEvent(PartyMemberId.None, NumericOperation.AddAmount, 250, 0)),
                ("change_experience None AddAmount 50 0", new ChangeExperienceEvent(PartyMemberId.None, NumericOperation.AddAmount, 50, 0)));
        }

        [Fact]
        public void ChangeFood()
        {
            Test(("change_food None AddAmount 6 0", new ChangeFoodEvent(PartyMemberId.None, NumericOperation.AddAmount, 6, 0)),
                ("change_food None SubtractAmount 1 1", new ChangeFoodEvent(PartyMemberId.None, NumericOperation.SubtractAmount, 1, 1)));
        }

        [Fact]
        public void ChangeGold()
        {
            Test(("change_gold None AddAmount 340 0", new ChangeGoldEvent(PartyMemberId.None, NumericOperation.AddAmount, 340, 0)));
        }

        [Fact]
        public void ChangeHealth()
        {
            Test(("change_health None AddAmount 2 1", new ChangeHealthEvent(PartyMemberId.None, NumericOperation.AddAmount, 2, 1)),
                ("change_health None AddPercentage 25 7", new ChangeHealthEvent(PartyMemberId.None, NumericOperation.AddPercentage, 25, 7)),
                ("change_health None SetToMaximum 0 0", new ChangeHealthEvent(PartyMemberId.None, NumericOperation.SetToMaximum, 0, 0)),
                ("change_health None SubtractAmount 17 1", new ChangeHealthEvent(PartyMemberId.None, NumericOperation.SubtractAmount, 17, 1)),
                ("change_health None SubtractPercentage 15 1", new ChangeHealthEvent(PartyMemberId.None, NumericOperation.SubtractPercentage, 15, 1)),
                ("change_health PartyMember.Harriet AddAmount 1 2", new ChangeHealthEvent(PartyMember.Harriet, NumericOperation.AddAmount, 1, 2)),
                ("change_health PartyMember.Unknown8 SetToMaximum 0 2", new ChangeHealthEvent(PartyMember.Unknown8, NumericOperation.SetToMaximum, 0, 2)),
                ("change_health PartyMember.Unknown11 SetToMaximum 0 2", new ChangeHealthEvent(PartyMember.Unknown11, NumericOperation.SetToMaximum, 0, 2)),
                ("change_health PartyMember.Unknown12 SetToMaximum 0 2", new ChangeHealthEvent(PartyMember.Unknown12, NumericOperation.SetToMaximum, 0, 2)));
        }

        [Fact]
        public void ChangeItem()
        {
            Test(("change_item None AddAmount 1 Item.BlueHealingPotion 0", new ChangeItemEvent(PartyMemberId.None, NumericOperation.AddAmount, 1, Item.BlueHealingPotion, 0)),
                ("change_item None SetToMinimum 0 Item.Canister 0", new ChangeItemEvent(PartyMemberId.None, NumericOperation.SetToMinimum, 0, Item.Canister, 0)),
                ("change_item None SetToMinimum 1 Item.StrengthAmulet 0", new ChangeItemEvent(PartyMemberId.None, NumericOperation.SetToMinimum, 1, Item.StrengthAmulet, 0)),
                ("change_item None SubtractAmount 1 Item.BlueStaff 0", new ChangeItemEvent(PartyMemberId.None, NumericOperation.SubtractAmount, 1, Item.BlueStaff, 0)),
                ("change_item PartyMember.Sira SubtractAmount 1 Item.TriifalaiSeed 2", new ChangeItemEvent(PartyMember.Sira, NumericOperation.SubtractAmount, 1, Item.TriifalaiSeed, 2)),
                ("change_item PartyMember.Tom AddAmount 1 Item.TheSeed 2", new ChangeItemEvent(PartyMember.Tom, NumericOperation.AddAmount, 1, Item.TheSeed, 2)));
        }

        [Fact]
        public void ChangeLanguage()
        {
            Test(("change_language None SetToMaximum Iskai 1", new ChangeLanguageEvent(PartyMemberId.None, NumericOperation.SetToMaximum, PlayerLanguages.Iskai, 1)),
                ("change_language PartyMember.Rainer SetToMaximum Iskai 2", new ChangeLanguageEvent(PartyMember.Rainer, NumericOperation.SetToMaximum, PlayerLanguages.Iskai, 2)),
                ("change_language PartyMember.Tom SetToMaximum Iskai 2", new ChangeLanguageEvent(PartyMember.Tom, NumericOperation.SetToMaximum, PlayerLanguages.Iskai, 2)));
        }

        [Fact]
        public void ChangeMana()
        {
            Test(("change_mana None AddPercentage 20 7", new ChangeManaEvent(PartyMemberId.None, NumericOperation.AddPercentage, 20, 7)),
                ("change_mana None AddPercentage 50 1", new ChangeManaEvent(PartyMemberId.None, NumericOperation.AddPercentage, 50, 1)),
                ("change_mana None SetToMaximum 0 0", new ChangeManaEvent(PartyMemberId.None, NumericOperation.SetToMaximum, 0, 0)),
                ("change_mana None SetToMaximum 0 1", new ChangeManaEvent(PartyMemberId.None, NumericOperation.SetToMaximum, 0, 1)));
        }

        [Fact]
        public void ChangeStatus()
        {
            Test(("change_status None SetToMaximum 0 Intoxicated 0", new ChangeStatusEvent(PartyMemberId.None, NumericOperation.SetToMaximum, 0, PlayerCondition.Intoxicated, 0)),
                ("change_status None SetToMaximum 0 Irritated 0", new ChangeStatusEvent(PartyMemberId.None, NumericOperation.SetToMaximum, 0, PlayerCondition.Irritated, 0)),
                ("change_status None SetToMaximum 3 Unconscious 1", new ChangeStatusEvent(PartyMemberId.None, NumericOperation.SetToMaximum, 3, PlayerCondition.Unconscious, 1)),
                ("change_status None SetToMinimum 0 Poisoned 7", new ChangeStatusEvent(PartyMemberId.None, NumericOperation.SetToMinimum, 0, PlayerCondition.Poisoned, 7)),
                ("change_status None SetToMinimum 10 Unconscious 0", new ChangeStatusEvent(PartyMemberId.None, NumericOperation.SetToMinimum, 10, PlayerCondition.Unconscious, 0)),
                ("change_status None SetToMinimum 3 Unconscious 1", new ChangeStatusEvent(PartyMemberId.None, NumericOperation.SetToMinimum, 3, PlayerCondition.Unconscious, 1)));
        }

        [Fact]
        public void ChangeUnkB()
        {
            Test(("change_unkb 115 SetAmount 116 0 5", new ChangeUnkBEvent(115, NumericOperation.SetAmount, 116, 0, 5)),
                ("change_unkb 220 SetAmount 220 228 5", new ChangeUnkBEvent(220, NumericOperation.SetAmount, 220, 228, 5)),
                ("change_unkb 237 SetAmount 255 0 5", new ChangeUnkBEvent(237, NumericOperation.SetAmount, 255, 0, 5)));
        }

        [Fact]
        public void DisableEventChain()
        {
            Test(@"disable_event_chain None 0 1 0
disable_event_chain None 0 1 1
disable_event_chain None 1 0 0
disable_event_chain None 1 0 1
disable_event_chain None 1 1 0
disable_event_chain None 1 1 1");
        }

        [Fact]
        public void DoScript()
        {
            Test(@"do_script Script.Unknown1");
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
            Test(@"inv:chest Chest.Unknown1 MapText.Jirinaar Item.Pistol 1 2 3
inv:chest Chest.Unknown1 MapText.Jirinaar None 0 255 255
inv:door Door.HerrasDoor MapText.Jirinaar Item.Pistol 1 2 3
inv:door Door.HerrasDoor MapText.Jirinaar None 0 255 255");
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
            Test((@"play_anim Video.MagicDemonstration 1 2 3 4", new PlayAnimationEvent(Video.MagicDemonstration, 1, 2, 3, 4)));
        }

        [Fact]
        public void Query()
        {
            Test(@"prompt_player MapText.TestMapIskai IsTrue 0 1
prompt_player EventText.FestivalTime IsTrue 1 1
prompt_player_numeric MapText.Jirinaar Equals 0 1
query_conscious IsTrue 0 PartyMember.Tom
query_conscious IsTrue 1 PartyMember.Tom
query_demo_version IsTrue 0 1
query_event_used IsTrue 0 0
query_gold GreaterThan 0 0
query_gold GreaterThanOrEqual 0 1
query_gold GreaterThanOrEqual 1 0
query_gold NotEqual 0 1
query_has_item Equals 1 Item.Pistol
query_has_item GreaterThan 0 Item.Pistol
query_has_item GreaterThanOrEqual 1 Item.Pistol
query_has_party_member IsTrue 0 PartyMember.Tom
query_leader Equals 0 PartyMember.Tom
query_leader IsTrue 0 PartyMember.Tom
query_map Equals 0 Map.1
query_npc_active Equals 0 0
query_npc_active Equals 1 1
query_npc_active IsTrue 0 0
query_npc_active IsTrue 0 1
query_npc_active IsTrue 1 0
query_previous_action_result IsTrue 0 0
query_random_chance IsTrue 1 1
query_random_chance NotEqual 0 1
query_random_chance NotEqual 1 1
query_random_chance OpUnk2 1 1
query_random_chance OpUnk6 0 1
query_script_debug_mode IsTrue 0 0
query_switch IsTrue 0 Switch.ExpelledFromSouthWind
query_ticker Equals 1 Ticker.Ticker100
query_ticker GreaterThan 1 Ticker.Ticker100
query_ticker GreaterThanOrEqual 1 Ticker.Ticker100
query_ticker NotEqual 1 Ticker.Ticker100
query_ticker OpUnk2 1 Ticker.Ticker100
query_unk1 Equals 1 1
query_unk1 Equals 1 1
query_unk1 GreaterThan 1 1
query_unk1 GreaterThan 1 1
query_unk1 GreaterThanOrEqual 1 1
query_unk1 GreaterThanOrEqual 1 1
query_unk1 IsTrue 0 0
query_unk1 IsTrue 1 0
query_unk1 NotEqual 1 1
query_unk1 NotEqual 1 1
query_unk1 OpUnk2 1 1
query_unk1 OpUnk6 1 1
query_unk1e GreaterThan 0 1
query_unk1e GreaterThanOrEqual 0 1
query_unk1e NotEqual 0 1
query_unk1e OpUnk6 0 1
query_unkc Equals 0 0
query_unkc Equals 0 1
query_used_item Equals 0 Item.Pistol
query_verb IsTrue 0 MapInit
query_used_item IsTrue 0 Item.Pistol");
        }

        [Fact]
        public void RemovePartyMember()
        {
            Test(@"remove_party_member None 1 1
remove_party_member PartyMember.Tom 1 1");
        }

        [Fact]
        public void SetMapLighting()
        {
            Test(@"set_map_lighting NeedTorch 1 2");
        }

        [Fact]
        public void NpcActive()
        {
            Test(@"npc_active 1 0 0 0
npc_active 1 1 0 0
npc_active 1 1 0 1");
        }

        [Fact]
        public void SetPartyLeader()
        {
            Test(@"set_party_leader PartyMember.Tom 3 0
set_party_leader PartyMember.Tom 1 1");
        }

        [Fact]
        public void SetTemporarySwitch()
        {
            Test(@"switch Reset Switch.ExpelledFromSouthWind 0
switch Reset Switch.ExpelledFromSouthWind 1
switch Set Switch.ExpelledFromSouthWind 0
switch Set Switch.ExpelledFromSouthWind 1
switch Toggle Switch.ExpelledFromSouthWind 0");
        }

        [Fact]
        public void Ticker()
        {
            Test(@"ticker Ticker.Ticker100 AddAmount 1 0
ticker Ticker.Ticker100 SetAmount 1 0
ticker Ticker.Ticker100 SetToMinimum 1 0
ticker Ticker.Ticker100 SubtractAmount 1 0");
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
            Test(@"sound None 0 0 0 0 GlobalOneShot
sound None 1 1 1 11000 GlobalOneShot
sound Sample.AmbientThrum 0 0 0 0 Silent
sound Sample.IllTemperedLlama 0 1 0 0 GlobalOneShot
sound Sample.IllTemperedLlama 0 1 1 0 GlobalOneShot
sound Sample.IllTemperedLlama 0 1 1 11000 GlobalOneShot
sound Sample.IllTemperedLlama 1 1 1 0 GlobalOneShot
sound Sample.IllTemperedLlama 1 1 0 11000 GlobalOneShot
sound Sample.IllTemperedLlama 1 1 1 11000 GlobalOneShot");
        }

        [Fact]
        public void Spinner()
        {
            Test(@"spinner 1");
        }

        [Fact]
        public void StartDialogue()
        {
            Test(@"start_dialogue Npc.Christine");
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
            Test(@"map_text EventText.Frill 1 1 None
map_text EventText.Frill 1 Conversation None
map_text EventText.Frill 1 ConversationOptions None
map_text EventText.Frill 1 ConversationQuery None
map_text EventText.Frill 1 NoPortrait None
map_text EventText.Frill 1 NoPortrait Npc.Christine
map_text EventText.Frill 1 PortraitLeft Npc.Christine
map_text EventText.Frill 1 PortraitLeft2 None
map_text EventText.Frill 1 StandardOptions None
map_text MapText.Jirinaar 1 1 None
map_text MapText.Jirinaar 1 NoPortrait None
map_text MapText.Jirinaar 1 PortraitLeft None
map_text MapText.Jirinaar 1 PortraitLeft Npc.Christine
map_text MapText.Jirinaar 1 PortraitLeft2 None
map_text MapText.Jirinaar 1 PortraitLeft3 Npc.Christine
map_text MapText.Jirinaar 1 QuickInfo None");
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
npc_text Npc.Khunagh 158
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
sound Sample.DistantCollapse 80 0 0 0 GlobalOneShot
sound Sample.DistantCollapse 80 0 0 7000 GlobalOneShot
sound Sample.DistantCollapse 80 150 0 0 GlobalOneShot
sound Sample.XylophoneTones 90 100 0 22000 GlobalOneShot
sound Sample.Healing 80 0 0 0 GlobalOneShot
sound_fx_off
start_anim 10 0 0 1
start_anim 3 25 24 2
stop_anim
text 101
update 1
update 200", true);

/* Original text w/ numeric ids
active_member_text 100
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
update 200
 */
        }
    }
}
