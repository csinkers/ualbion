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

        static void Test(string data)
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
            MapEvent.Serdes(e, s, TextId.None, AssetMapping.Global);
            bw.Flush();
            ms.Position = 0;
            return ms.ToArray();
        }

        static IMapEvent BytesToEvent(byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            using var br = new BinaryReader(ms);
            using var s = new AlbionReader(br);
            return MapEvent.Serdes(null, s, TextId.None, AssetMapping.Global);
        }

        static void Test(params (string, IMapEvent)[] events)
        {
            var results = new List<string>();

            foreach (var (line, e) in events)
            {
                if (!string.Equals(e.ToString(), line, StringComparison.OrdinalIgnoreCase))
                    results.Add($"Event \"{e}\" did not serialise to the expected string \"{line}\"");

                IMapEvent parsed = null;
                try { parsed = (IMapEvent)Event.Parse(line); }
                catch (Exception ex) { results.Add($"Could not parse \"{line}\": {ex}"); }

                if (parsed == null)
                    continue;

                var bytes1 = EventToBytes(e);
                var bytes2 = EventToBytes(parsed);
                var hex1 = FormatUtil.BytesToHexString(bytes1);
                var hex2 = FormatUtil.BytesToHexString(bytes2);
                if (!string.Equals(hex1, hex2, StringComparison.Ordinal))
                    results.Add($"The literal event (\"{e}\") serialised to {hex1}, but the parsed event (\"{line}\") serialised to {hex2}");
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
            Test(("DummyModifyEvent", new DummyModifyEvent()));
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
                ("action Unk23 1 Unknown.1 1",          new ActionEvent(ActionType.Unk14,          1, new AssetId(AssetType.Unknown, 1), 1)),
                ("action UseItem 1 Item.Pistol 1",      new ActionEvent(ActionType.UseItem,        1, (ItemId)Item.Pistol, 1)),
                ("action Word 1 Unknown.0 1",           new ActionEvent(ActionType.Word,           1, AssetId.None, 1)),
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
            Test(("add_remove_inv_item AddAmount 1 Item.Pistol", new AddRemoveInventoryItemEvent(NumericOperation.AddAmount, 1, Item.Pistol)),
                ("add_remove_inv_item SetToMinimum 0 Item.Pistol", new AddRemoveInventoryItemEvent(NumericOperation.SetToMinimum, 0, Item.Pistol)),
                ("add_remove_inv_item SetToMinimum 1 Item.Pistol", new AddRemoveInventoryItemEvent(NumericOperation.SetToMinimum, 1, Item.Pistol)),
                ("add_remove_inv_item SubtractAmount 1 Item.Pistol", new AddRemoveInventoryItemEvent(NumericOperation.SubtractAmount, 1, Item.Pistol)));
        }

        [Fact]
        public void ChangeIcon()
        {
            Test(("change_icon 1 1 0 BlockHard 0", new ChangeIconEvent(1, 1, 0, IconChangeType.BlockHard, 0)),
                ("change_icon 1 1 0 BlockHard 1", new ChangeIconEvent(1, 1, 0, IconChangeType.BlockHard, 1)),
                ("change_icon 1 1 Rel BlockHard 1", new ChangeIconEvent(1, 1, EventScopes.Rel, IconChangeType.BlockHard, 1)),
                ("change_icon 1 1 Rel, Temp BlockHard 1", new ChangeIconEvent(1, 1, EventScopes.Rel | EventScopes.Temp, IconChangeType.BlockHard, 1)),
                ("change_icon 1 1 Temp BlockHard 1", new ChangeIconEvent(1, 1, EventScopes.Temp, IconChangeType.BlockHard, 1)));
        }

        [Fact]
        public void ChangePartyGold()
        {
            Test(("change_party_gold AddAmount 1 0", new ChangePartyGoldEvent(NumericOperation.AddAmount, 1, 0)),
                ("change_party_gold AddAmount 1 1", new ChangePartyGoldEvent(NumericOperation.AddAmount, 1, 0)),
                ("change_party_gold SetToMinimum 0 0", new ChangePartyGoldEvent(NumericOperation.SetToMinimum, 0, 0)),
                ("change_party_gold SubtractAmount 0 1", new ChangePartyGoldEvent(NumericOperation.SubtractAmount, 0, 1)),
                ("change_party_gold SubtractAmount 1 0", new ChangePartyGoldEvent(NumericOperation.SubtractAmount, 1, 0)),
                ("change_party_gold SubtractAmount 1 1", new ChangePartyGoldEvent(NumericOperation.SubtractAmount, 1, 1)),
                ("change_party_rations AddAmount 1 1", new ChangePartyGoldEvent(NumericOperation.AddAmount, 1, 1)));
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
            Test(("clone_automap 122 164", new CloneAutomapEvent(Map.OldFormerBuilding, Map.OldFormerBuildingPostFight)));
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
            Test(("change_gold None AddAmount 340 0 0", new ChangeGoldEvent(PartyMemberId.None, NumericOperation.AddAmount, 340, 0)));
        }

        [Fact]
        public void ChangeHealth()
        {
            Test(("change_health None AddAmount 2 0 1", new ChangeHealthEvent(PartyMemberId.None, NumericOperation.AddAmount, 2, 1)),
                ("change_health None AddPercentage 25 0 7", new ChangeHealthEvent(PartyMemberId.None, NumericOperation.AddPercentage, 25, 7)),
                ("change_health None SetToMaximum 0 0 0", new ChangeHealthEvent(PartyMemberId.None, NumericOperation.SetToMaximum, 0, 0)),
                ("change_health None SubtractAmount 17 0 1", new ChangeHealthEvent(PartyMemberId.None, NumericOperation.SubtractAmount, 17, 1)),
                ("change_health None SubtractPercentage 15 0 1", new ChangeHealthEvent(PartyMemberId.None, NumericOperation.SubtractPercentage, 15, 1)),
                ("change_health PartyMember.Harriet AddAmount 1 0 2", new ChangeHealthEvent(PartyMember.Harriet, NumericOperation.AddAmount, 1, 2)),
                ("change_health PartyMember.Unknown8 SetToMaximum 0 0 2", new ChangeHealthEvent(PartyMember.Unknown8, NumericOperation.SetToMaximum, 0, 2)),
                ("change_health PartyMember.Unknown11 SetToMaximum 0 0 2", new ChangeHealthEvent(PartyMember.Unknown11, NumericOperation.SetToMaximum, 0, 2)),
                ("change_health PartyMember.Unknown12 SetToMaximum 0 0 2", new ChangeHealthEvent(PartyMember.Unknown12, NumericOperation.SetToMaximum, 0, 2)));
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
            Test(("change_language None SetToMaximum 0 Iskai 1", new ChangeLanguageEvent(PartyMemberId.None, NumericOperation.SetToMaximum, PlayerLanguages.Iskai, 1)),
                ("change_language PartyMember.Rainer SetToMaximum 0 Iskai 2", new ChangeLanguageEvent(PartyMember.Rainer, NumericOperation.SetToMaximum, PlayerLanguages.Iskai, 2)),
                ("change_language PartyMember.Tom SetToMaximum 0 Iskai 2", new ChangeLanguageEvent(PartyMember.Tom, NumericOperation.SetToMaximum, PlayerLanguages.Iskai, 2)));
        }

        [Fact]
        public void ChangeMana()
        {
            Test(("change_mana None AddPercentage 20 0 7", new ChangeManaEvent(PartyMemberId.None, NumericOperation.AddPercentage, 20, 7)),
                ("change_mana None AddPercentage 50 0 1", new ChangeManaEvent(PartyMemberId.None, NumericOperation.AddPercentage, 50, 1)),
                ("change_mana None SetToMaximum 0 0 0", new ChangeManaEvent(PartyMemberId.None, NumericOperation.SetToMaximum, 0, 0)),
                ("change_mana None SetToMaximum 0 0 1", new ChangeManaEvent(PartyMemberId.None, NumericOperation.SetToMaximum, 0, 1)));
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
            Test(@"disable_event_chain 0 (1 0 0 0 0)
disable_event_chain 0 (1 0 0 1 0)
disable_event_chain 1 (0 0 0 0 0)
disable_event_chain 1 (0 0 0 1 0)
disable_event_chain 1 (1 0 0 0 0)
disable_event_chain 1 (1 0 0 1 0)");
        }

        [Fact]
        public void DoScript()
        {
            Test(@"do_script Script.Unknown1");
        }

        [Fact]
        public void EncounterEvent()
        {
            Test(@"encounter_event (0 0 0 0 0 1 1)");
        }

        [Fact]
        public void EndDialogue()
        {
            Test(@"end_dialogue");
        }

        [Fact]
        public void Execute()
        {
            Test(@"execute (0 0 0 0 0 0 1)
execute (1 0 0 0 0 0 1)");
        }

        [Fact]
        public void Inv()
        {
            Test(@"inv:chest Chest.Unknown1 1% Initial:1 Unlocked:1 Key:Item.Pistol
inv:chest Chest.Unknown1 1% Initial:1 Unlocked:1 Key:None
inv:door Door.HerrasDoor 1% Initial:1 Unlocked:1 Key:Item.Pistol
inv:door Door.HerrasDoor 1% Initial:1 Unlocked:1 Key:None");
        }

        [Fact]
        public void Pause()
        {
            Test(@"pause 1");
        }

        [Fact]
        public void PlaceAction()
        {
            Test(@"place_action AskOpinion (0 0 0 0 1 1)
place_action AskOpinion (1 1 1 1 1 1)
place_action Cure (1 1 1 1 1 1)
place_action Heal (1 1 1 1 1 1)
place_action LearnCloseCombat (1 1 1 0 1 0)
place_action LearnCloseCombat (1 1 1 1 1 0)
place_action LearnSpells (1 1 1 1 1 1)
place_action Merchant (1 1 1 0 1 1)
place_action Merchant (1 1 1 1 1 1)
place_action OrderFood (1 1 1 0 1 0)
place_action OrderFood (1 1 1 0 1 1)
place_action OrderFood (1 1 1 1 1 0)
place_action OrderFood (1 1 1 1 1 1)
place_action RemoveCurse (1 1 1 0 0 0)
place_action RemoveCurse (1 1 1 0 0 1)
place_action RemoveCurse (1 1 1 0 1 0)
place_action RemoveCurse (1 1 1 1 1 1)
place_action RepairItem (1 1 1 0 1 0)
place_action RepairItem (1 1 1 0 1 1)
place_action RepairItem (1 1 1 1 1 0)
place_action RestoreItemEnergy (1 1 1 0 1 0)
place_action ScrollMerchant (1 1 1 0 1 1)
place_action SleepInRoom (1 1 1 1 1 0)
place_action SleepInRoom (1 1 1 1 1 1)");
        }

        [Fact]
        public void PlayAnim()
        {
            Test(@"play_anim Video.MagicDemonstration (1, 1) 0 0 1 1)");
        }

        [Fact]
        public void Query()
        {
            Test(@"query 1 1 (Equals 1)
query 1 1 (GreaterThan 1)
query 1 1 (GreaterThanOrEqual 1)
query 1 1 (NotEqual 1)
query 1 1 (OpUnk1 1)
query ChosenVerb MapInit (IsTrue 0)
query CurrentMapId 1 (Equals 0)
query EventAlreadyUsed 0 (IsTrue 0)
query HasEnoughGold 0 (GreaterThan 0)
query HasEnoughGold 0 (GreaterThanOrEqual 1)
query HasEnoughGold 1 (GreaterThanOrEqual 0)
query HasEnoughGold 1 (NotEqual 0)
query HasPartyMember PartyMember.Tom (IsTrue 0)
query InventoryHasItem Item.Pistol (Equals 1)
query InventoryHasItem Item.Pistol (GreaterThan 0)
query InventoryHasItem Item.Pistol (GreaterThanOrEqual 1)
query IsDemoVersion 1 (IsTrue 0)
query IsNpcActive 0 (Equals 0)
query IsNpcActive 0 (IsTrue 0)
query IsNpcActive 0 (IsTrue 1)
query IsNpcActive 1 (Equals 1)
query IsNpcActive 1 (IsTrue 0)
query IsPartyMemberConscious PartyMember.Tom (IsTrue 0)
query IsPartyMemberConscious PartyMember.Tom (IsTrue 1)
query IsPartyMemberLeader PartyMember.Tom (Equals 0)
query IsPartyMemberLeader PartyMember.Tom (IsTrue 0)
query IsScriptDebugModeActive 0 (IsTrue 0)
query PreviousActionResult 0 (IsTrue 0)
query PromptPlayer 1 (IsTrue 0)
query PromptPlayer 1 (IsTrue 1)
query PromptPlayerNumeric 1 (Equals 0)
query RandomChance 1 (IsTrue 1)
query RandomChance 1 (NotEqual 0)
query RandomChance 1 (NotEqual 1)
query RandomChance 1 (OpUnk1 0)
query RandomChance 1 (OpUnk1 1)
query TemporarySwitch Switch.ExpelledFromSouthWind (IsTrue 0)
query Ticker Ticker.Ticker1 (Equals 1)
query Ticker Ticker.Ticker1 (GreaterThan 1)
query Ticker Ticker.Ticker1 (GreaterThanOrEqual 1)
query Ticker Ticker.Ticker1 (NotEqual 1)
query Ticker Ticker.Ticker1 (OpUnk1 1)
query Unk1 0 (IsTrue 0)
query Unk1 0 (IsTrue 1)
query Unk1 1 (Equals 1)
query Unk1 1 (GreaterThan 1)
query Unk1 1 (GreaterThanOrEqual 1)
query Unk1 1 (NotEqual 1)
query Unk1 1 (OpUnk1 1)
query Unk1E 1 (GreaterThan 0)
query Unk1E 1 (GreaterThanOrEqual 0)
query Unk1E 1 (NotEqual 0)
query Unk1E 1 (OpUnk1 0)
query UnkC 0 (Equals 0)
query UnkC 1 (Equals 0)
query UsedItemId Item.Pistol (Equals 0)
query UsedItemId Item.Pistol (IsTrue 0)");
        }

        [Fact]
        public void RemovePartyMember()
        {
            Test(@"remove_party_member (None 1 0 1)
remove_party_member (PartyMember.Tom 1 0 1)");
        }

        [Fact]
        public void SetMapLighting()
        {
            Test(@"set_map_lighting NeedTorch (1 1 0 0 0)");
        }

        [Fact]
        public void SetNpcActive()
        {
            Test(@"set_npc_active 1 0 (0 0 0)
set_npc_active 1 1 (0 0 0)
set_npc_active 1 1 (0 1 0)");
        }

        [Fact]
        public void SetPartyLeader()
        {
            Test(@"set_party_leader PartyMember.Tom (1 0 0 0 0)
set_party_leader PartyMember.Tom (1 1 0 0 0)");
        }

        [Fact]
        public void SetTemporarySwitch()
        {
            Test(@"set_temporary_switch Switch.ExpelledFromSouthWind Reset (0)
set_temporary_switch Switch.ExpelledFromSouthWind Reset (1)
set_temporary_switch Switch.ExpelledFromSouthWind Set (0)
set_temporary_switch Switch.ExpelledFromSouthWind Set (1)
set_temporary_switch Switch.ExpelledFromSouthWind Toggle (0)");
        }

        [Fact]
        public void SetTicker()
        {
            Test(@"set_ticker Ticker.Ticker1 AddAmount 1 (0)
set_ticker Ticker.Ticker1 SetAmount 1 (0)
set_ticker Ticker.Ticker1 SetToMinimum 1 (0)
set_ticker Ticker.Ticker1 SubtractAmount 1 (0)");
        }

        [Fact]
        public void Signal()
        {
            Test(@"signal 1");
        }

        [Fact]
        public void SimpleChest()
        {
            Test(@"simple_chest Gold 1xNone
simple_chest Item 1xItem.Pistol
simple_chest Item 1xNone
simple_chest Rations 1xNone");
        }

        [Fact]
        public void Sound()
        {
            Test(@"sound None GlobalOneShot Vol:0 Prob:0% Freq:0 (0)
sound None GlobalOneShot Vol:1 Prob:1% Freq:1 (1)
sound Sample.AmbientThrum Silent Vol:0 Prob:0% Freq:0 (0)
sound Sample.IllTemperedLlama GlobalOneShot Vol:0 Prob:1% Freq:0 (0)
sound Sample.IllTemperedLlama GlobalOneShot Vol:0 Prob:1% Freq:0 (1)
sound Sample.IllTemperedLlama GlobalOneShot Vol:0 Prob:1% Freq:1 (1)
sound Sample.IllTemperedLlama GlobalOneShot Vol:1 Prob:1% Freq:0 (1)
sound Sample.IllTemperedLlama GlobalOneShot Vol:1 Prob:1% Freq:1 (0)
sound Sample.IllTemperedLlama GlobalOneShot Vol:1 Prob:1% Freq:1 (1)");
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
            Test(@"teleport Map.Jirinaar <1, 1> Dir:Up (1 0)
teleport Map.Jirinaar <1, 1> Dir:Up (1 1)
teleport None <1, 1> Dir:Up (1 1)");
        }

        [Fact]
        public void Text()
        {
            Test(@"text EventText.Frill:1 1 None (0 0 0 0)
text EventText.Frill:1 Conversation None (0 0 0 0)
text EventText.Frill:1 ConversationOptions None (0 0 0 0)
text EventText.Frill:1 ConversationQuery None (0 0 0 0)
text EventText.Frill:1 NoPortrait None (0 0 0 0)
text EventText.Frill:1 NoPortrait Npc.Christine (0 0 0 0)
text EventText.Frill:1 PortraitLeft Npc.Christine (0 0 0 0)
text EventText.Frill:1 PortraitLeft1 None (0 0 0 0)
text EventText.Frill:1 StandardOptions None (0 0 0 0)
text MapText.Jirinaar:1 1 None (0 0 0 0)
text MapText.Jirinaar:1 NoPortrait None (0 0 0 0)
text MapText.Jirinaar:1 PortraitLeft None (0 0 0 0)
text MapText.Jirinaar:1 PortraitLeft Npc.Christine (0 0 0 0)
text MapText.Jirinaar:1 PortraitLeft1 None (0 0 0 0)
text MapText.Jirinaar:1 PortraitLeft1 Npc.Christine (0 0 0 0)
text MapText.Jirinaar:1 QuickInfo None (0 0 0 0)");
        }

        [Fact]
        public void Trap()
        {
            Test(@"trap (1 1 0 0 0)
trap (1 1 0 0 1)
trap (1 1 1 0 1)
trap (1 1 1 1 0)
trap (1 1 1 1 1)");
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
            Test(@"active_member_text 100
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
update 200");
        }
    }
}
