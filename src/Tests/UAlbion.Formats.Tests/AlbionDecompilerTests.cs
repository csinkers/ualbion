using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Scripting;
using UAlbion.Scripting.Ast;
using UAlbion.TestCommon;
using Xunit;

namespace UAlbion.Formats.Tests
{
    public class AlbionDecompilerTests
    {
        const string Sira2EventSetHex = @"
0f00 7200 0000 1d00 2100 2500 2700 2b00 2d00 2f00 3100 3300 3a00 5000 5800 6400 7000 0e06 0100 0000 0000 
0000 0100 0c05 0000 0000 0400 0b00 0200 0c00 0000 0000 5502 0300 0600 040d 0000 0007 0000 0000 0400 040b 
0000 0001 0000 0000 0500 0404 0000 0001 0000 0000 ffff 0c00 0000 0000 4d00 0800 0700 040d 0000 000b 0000 
0000 0900 040d 0000 0007 0000 0000 0900 040b 0000 0008 0000 0000 0a00 0404 0000 0008 0000 0000 ffff 0c00 
0000 0000 4c00 0d00 0c00 0400 0000 0010 0000 0000 0e00 0400 0000 000f 0000 0000 0e00 0d00 0100 0000 4c00 
0000 0f00 0c1f 0000 0000 1100 1000 1200 0400 0000 0014 0000 0000 1100 1600 0000 0000 0000 0000 ffff 0400 
0000 0013 0000 0000 1300 0d05 0300 0000 0400 0000 1400 0c09 0000 0000 0000 1700 1500 0d05 0300 0000 0500 
0000 1600 0c09 0000 0000 0000 1700 1800 0400 0000 0012 0000 0000 1b00 0d04 0108 0000 1901 0000 1900 0d04 
0109 0000 1901 0000 1a00 1600 0000 0000 0000 0000 ffff 1504 0100 0000 0869 0000 1c00 1505 0100 0000 0969 
0000 ffff 0e08 010a 0000 0100 0000 1e00 0c22 0000 0000 0000 1f00 2000 0400 0000 0002 0000 0000 ffff 0400 
0000 0003 0000 0000 ffff 0e08 010a 0000 0800 0000 2200 0c22 0000 0000 0000 2300 2400 0400 0000 0002 0000 
0000 ffff 0400 0000 0003 0000 0000 ffff 0e08 010b 0000 0800 0000 2600 0400 0000 0009 0000 0000 ffff 0e00 
0100 0000 fd01 0000 2800 0c22 0000 0000 0000 2a00 2900 0400 0000 0005 0000 0000 ffff 0400 0000 0004 0000 
0000 ffff 0e00 0100 0000 ff01 0000 2c00 0400 0000 0006 0000 0000 ffff 0e00 0100 0000 0f02 0000 2e00 0400 
0000 0006 0000 0000 ffff 0e00 0100 0000 4902 0000 3000 0400 0000 000a 0000 0000 ffff 0e01 01ff 0000 5801 
0000 3200 0400 0000 000a 0000 0000 ffff 0e05 0100 0000 0000 0000 3400 0c1f 0000 0000 0c00 3500 3600 0400 
0000 000e 0000 0000 ffff 0400 0000 000d 0000 0000 3700 1504 0100 0000 0869 0000 3800 1505 0100 0000 0969 
0000 3900 1600 0000 0000 0000 0000 ffff 0c00 0000 0000 0000 ffff ffff 0e06 0100 0000 0000 0000 3c00 0c05 
0000 0000 0300 4700 3d00 0c00 0000 0000 5702 3e00 4100 040d 0000 000c 0000 0000 3f00 040b 0000 0001 0000 
0000 4000 0404 0000 0001 0000 0000 ffff 0c00 0000 0000 3000 4200 4300 0c00 0000 0000 3100 4400 4300 040d 
0000 0014 0000 0000 4500 040d 0000 000d 0000 0000 4500 040b 0000 0002 0000 0000 4600 0404 0000 0002 0000 
0000 ffff 0c22 0000 0000 0000 4900 4800 0400 0000 0019 0000 0000 4a00 0400 0000 0018 0000 0000 4a00 0c1f 
0000 0000 1a00 4b00 4d00 0400 0000 001c 0000 0000 4c00 1600 0000 0000 0000 0000 ffff 0400 0000 001b 0000 
0000 4e00 0d05 0300 0000 0300 0000 4f00 1600 0000 0000 0000 0000 ffff 0e3d 0100 0000 0000 0000 5100 0c05 
0000 0000 0500 ffff 5200 0c00 0000 0000 5502 5600 ffff 0802 0402 0004 0000 0200 5400 0802 0402 0005 0000 
0200 5500 0c15 0000 0000 0500 ffff 5600 1d00 0000 0000 3c00 0000 5700 0d00 0100 0000 5502 0000 ffff 0e2d 
01ff 0000 0000 0000 5900 0813 0502 0004 5801 0100 5a00 0c09 0000 0000 0000 5b00 ffff 1401 0000 0000 0000 
ffff 5c00 0402 0000 0415 0000 0000 5d00 0c00 0000 0000 4a00 5e00 ffff 0c15 0000 0000 0100 ffff 5f00 0402 
0000 0116 0000 0000 6000 0402 0000 0417 0000 0000 6100 0402 0000 0118 0000 0000 6200 0402 0000 0419 0000 
0000 6300 0d00 0100 0000 4a00 0000 ffff 0e17 01ff 0000 0000 0000 6500 0813 0502 0004 5801 0100 6600 0c09 
0000 0000 0000 6700 ffff 1401 0000 0000 0000 ffff 6800 0402 0000 0415 0000 0000 6900 0c00 0000 0000 4a00 
6a00 ffff 0c15 0000 0000 0100 ffff 6b00 0402 0000 0116 0000 0000 6c00 0402 0000 0417 0000 0000 6d00 0402 
0000 0118 0000 0000 6e00 0402 0000 0419 0000 0000 6f00 0d00 0100 0000 4a00 0000 ffff 0e00 0100 0000 2102 
0000 7100 0400 0000 0026 0000 0000 ffff
";

        static void Verify(ICfgNode tree, List<(string, IGraph)> steps, string expected, [CallerMemberName] string method = null)
        {
            var visitor = new FormatScriptVisitor();
            tree.Accept(visitor);
            DumpSteps(steps, method);

            Assert.Equal(expected, visitor.Code);
        }

        static void DumpSteps(List<(string, IGraph)> steps, string method)
        {
            if (steps == null)
                return;

            var disk = new MockFileSystem(true);
            var baseDir = ConfigUtil.FindBasePath(disk);
            var resultsDir = Path.Combine(baseDir, "re", "DecompilerTests");
            if (!Directory.Exists(resultsDir))
                Directory.CreateDirectory(resultsDir);

            for (int i = 0; i < steps.Count; i++)
            {
                var (description, graph) = steps[i];
                var path = Path.Combine(resultsDir, $"{method}_{i}_{description}.gv");
                File.WriteAllText(path, graph.ExportToDot());

                var graphVizDot = @"C:\Program Files\Graphviz\bin\dot.exe";
                if (!File.Exists(graphVizDot))
                    continue;

                var pngPath = Path.ChangeExtension(path, "png");
                var args = $"\"{path}\" -T png -o \"{pngPath}\"";
                using var process = Process.Start(graphVizDot, args);
                process?.WaitForExit();
            }
        }

        static void TestDecompile(string script, string expected, [CallerMemberName] string method = null)
        {
            var events = EventNode.ParseRawEvents(script);
            var steps = new List<(string, IGraph)>();
            try
            {
                var result = Decompiler.Decompile(
                    events,
                    new ushort[] { 0 },
                    null,
                    steps).Single();

                Verify(result, steps, expected, method);
            }
            catch (ControlFlowGraphException e)
            {
                steps.Add((e.Message, e.Graph));
                DumpSteps(steps, method);
                throw;
            }
            catch
            {
                DumpSteps(steps, method);
                throw;
            }
        }

        static void Setup()
        {
            AssetMapping.GlobalIsThreadLocal = true;
            AssetMapping.Global.RegisterAssetType(typeof(Base.Door), AssetType.Door);
            AssetMapping.Global.RegisterAssetType(typeof(Base.EventSet), AssetType.EventSet);
            AssetMapping.Global.RegisterAssetType(typeof(Base.EventText), AssetType.EventText);
            AssetMapping.Global.RegisterAssetType(typeof(Base.Item), AssetType.Item);
            AssetMapping.Global.RegisterAssetType(typeof(Base.Map), AssetType.Map);
            AssetMapping.Global.RegisterAssetType(typeof(Base.MapText), AssetType.MapText);
            AssetMapping.Global.RegisterAssetType(typeof(Base.PartyMember), AssetType.Party);
            AssetMapping.Global.RegisterAssetType(typeof(Base.Switch), AssetType.Switch);
            AssetMapping.Global.RegisterAssetType(typeof(Base.Ticker), AssetType.Ticker);
            AssetMapping.Global.RegisterAssetType(typeof(Base.Word), AssetType.Word);
            Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(ScriptEvents.PartyMoveEvent)));
        }

        [Fact]
        public void DecompileTest()
        {
            Setup();

            const string script = 
@"!0?1:2: query_verb Examine
 1=>!: map_text MapText.Jirinaar 37 NoPortrait None ; ""The door to the house of the Hunter Clan. It is secured with a lock.""
!2?3:!: open_door Door.HunterClan MapText.Jirinaar Item.HunterClanKey 100 32 33
!3?4:!: result
 4=>5: modify_unk2 0 0 0 0 101 0
 5=>!: teleport Map.HunterClan 69 67 Unchanged 255 0";

            string expected = 
@"Chain0:
if (query_verb Examine) {
    map_text MapText.Jirinaar 37
} else {
    if (open_door Door.HunterClan MapText.Jirinaar Item.HunterClanKey 100 32 33) {
        if (result) {
            modify_unk2 0 0 0 0 101 0
            teleport Map.HunterClan 69 67 Unchanged 255 0
        }
    }
}";
            TestDecompile(script, expected);
        }

        [Fact]
        public void SiraParseTest()
        { // From event set 984 (Sira2)
            Setup();
            var bytes = FormatUtil.HexStringToBytes(Sira2EventSetHex.Replace(" ", "").Replace(Environment.NewLine, ""));
            var set = FormatUtil.DeserializeFromBytes(bytes,
                s => EventSet.Serdes(Base.EventSet.Sira2, null, AssetMapping.Global, s));
            var script = string.Join(Environment.NewLine, set.EventStrings);

            const string expectedScript = @" 0=>1: action StartDialogue
!1?2:11: party_has PartyMember.Sira
!2?6:3: get_switch Switch.Switch597
 3=>4: map_text EventText.Sira2 7 StandardOptions
 4=>5: map_text EventText.Sira2 1 ConversationOptions
 5=>!: map_text EventText.Sira2 1 Conversation
!6?7:8: get_switch Switch.Switch77
#7=>9: map_text EventText.Sira2 11 StandardOptions
 8=>9: map_text EventText.Sira2 7 StandardOptions
 9=>10: map_text EventText.Sira2 8 ConversationOptions
 10=>!: map_text EventText.Sira2 8 Conversation
!11?12:13: get_switch Switch.Switch76
#12=>14: map_text EventText.Sira2 16
 13=>14: map_text EventText.Sira2 15
 14=>15: switch Set Switch.Switch76
!15?18:16: prompt_player EventText.Sira2 17
 16=>17: map_text EventText.Sira2 20
 17=>!: end_dialogue
 18=>19: map_text EventText.Sira2 19
 19=>20: add_party_member PartyMember.Sira 3
!20?21:23: result
 21=>22: add_party_member PartyMember.Mellthas 3
!22?24:23: result
#23=>27: map_text EventText.Sira2 18
 24=>25: npc_active 8 1 0 281
 25=>26: npc_active 9 1 0 281
 26=>!: end_dialogue
 27=>28: remove_party_member PartyMember.Sira 1 26888
 28=>!: remove_party_member PartyMember.Mellthas 1 26889
 29=>30: action DialogueLine 10 Unknown.1
!30?32:31: event_used
 31=>!: map_text EventText.Sira2 2
 32=>!: map_text EventText.Sira2 3
 33=>34: action DialogueLine 10 Unknown.8
!34?36:35: event_used
 35=>!: map_text EventText.Sira2 2
 36=>!: map_text EventText.Sira2 3
 37=>38: action DialogueLine 11 Unknown.8
 38=>!: map_text EventText.Sira2 9
 39=>40: action Word 0 Word.Former
!40?41:42: event_used
 41=>!: map_text EventText.Sira2 5
 42=>!: map_text EventText.Sira2 4
 43=>44: action Word 0 Word.Magician
 44=>!: map_text EventText.Sira2 6
 45=>46: action Word 0 Word.Guild1
 46=>!: map_text EventText.Sira2 6
 47=>48: action Word 0 Word.GuildHouse
 48=>!: map_text EventText.Sira2 10
 49=>50: action AskAboutItem 255 Item.TriifalaiSeed
 50=>!: map_text EventText.Sira2 10
 51=>52: action AskToLeave
!52?54:53: prompt_player EventText.Sira2 12
 53=>!: map_text EventText.Sira2 14
 54=>55: map_text EventText.Sira2 13
 55=>56: remove_party_member PartyMember.Sira 1 26888
 56=>57: remove_party_member PartyMember.Mellthas 1 26889
 57=>!: end_dialogue
!58?!:!: get_switch Switch.Switch0
 59=>60: action StartDialogue
!60?61:71: party_has PartyMember.Drirr
!61?65:62: get_switch Switch.Switch599
 62=>63: map_text EventText.Sira2 12 StandardOptions
 63=>64: map_text EventText.Sira2 1 ConversationOptions
 64=>!: map_text EventText.Sira2 1 Conversation
!65?67:66: get_switch Switch.OnMissionToObtainHighKnowledge
!66?67:68: get_switch Switch.OnMissionToDestroyShip
#67=>69: map_text EventText.Sira2 20 StandardOptions
 68=>69: map_text EventText.Sira2 13 StandardOptions
 69=>70: map_text EventText.Sira2 2 ConversationOptions
 70=>!: map_text EventText.Sira2 2 Conversation
!71?72:73: event_used
#72=>74: map_text EventText.Sira2 25
 73=>74: map_text EventText.Sira2 24
!74?77:75: prompt_player EventText.Sira2 26
 75=>76: map_text EventText.Sira2 28
 76=>!: end_dialogue
 77=>78: map_text EventText.Sira2 27
 78=>79: add_party_member PartyMember.Drirr 3
 79=>!: end_dialogue
 80=>81: action Unk3D
!81?82:!: party_has PartyMember.Mellthas
!82?!:86: get_switch Switch.Switch597
 83=>84: change_health PartyMember.Sira AddAmount 2 2
 84=>85: change_health PartyMember.Mellthas AddAmount 2 2
!85?86:!: is_conscious PartyMember.Mellthas
 86=>87: do_script Script.60
 87=>!: switch Set Switch.Switch597
 88=>89: action Unk2D 255
 89=>90: change_item PartyMember.Sira SubtractAmount 1 Item.TriifalaiSeed 2
!90?!:91: result
 91=>92: execute 1 65535
 92=>93: map_text EventText.Sira2 21 PortraitLeft PartyMember.Sira
!93?!:94: get_switch Switch.SiraAndTomDiscussedSeedSignificance
!94?95:!: is_conscious PartyMember.Tom
 95=>96: map_text EventText.Sira2 22 PortraitLeft PartyMember.Tom
 96=>97: map_text EventText.Sira2 23 PortraitLeft PartyMember.Sira
 97=>98: map_text EventText.Sira2 24 PortraitLeft PartyMember.Tom
 98=>99: map_text EventText.Sira2 25 PortraitLeft PartyMember.Sira
 99=>!: switch Set Switch.SiraAndTomDiscussedSeedSignificance
 100=>101: action Unk17 255
 101=>102: change_item PartyMember.Sira SubtractAmount 1 Item.TriifalaiSeed 2
!102?!:103: result
 103=>104: execute 1 65535
 104=>105: map_text EventText.Sira2 21 PortraitLeft PartyMember.Sira
!105?!:106: get_switch Switch.SiraAndTomDiscussedSeedSignificance
!106?107:!: is_conscious PartyMember.Tom
 107=>108: map_text EventText.Sira2 22 PortraitLeft PartyMember.Tom
 108=>109: map_text EventText.Sira2 23 PortraitLeft PartyMember.Sira
 109=>110: map_text EventText.Sira2 24 PortraitLeft PartyMember.Tom
 110=>111: map_text EventText.Sira2 25 PortraitLeft PartyMember.Sira
 111=>!: switch Set Switch.SiraAndTomDiscussedSeedSignificance
 112=>113: action Word 0 Word.Stri
 113=>!: map_text EventText.Sira2 38";

            var lines = ApiUtil.SplitLines(script);
            var expectedLines = ApiUtil.SplitLines(expectedScript);
            
            Assert.Equal(lines.Length, expectedLines.Length);

            var discrep = new List<string>();
            for (int i = 0; i < lines.Length; i++)
                if (lines[i] != expectedLines[i])
                    discrep.Add($"{i}: Expected \"{expectedLines[i]}\", but was \"{lines[i]}\"");

            if (discrep.Any())
                throw new InvalidOperationException( string.Join(Environment.NewLine, discrep));
        }

        [Fact]
        public void SiraDecompileTest()
        {
            Setup();
            var bytes = FormatUtil.HexStringToBytes(Sira2EventSetHex.Replace(" ", "").Replace(Environment.NewLine, ""));
            var set = FormatUtil.DeserializeFromBytes(bytes, s => EventSet.Serdes(Base.EventSet.Sira2, null, AssetMapping.Global, s));
            var script = string.Join(Environment.NewLine, set.EventStrings.Take(29));

            const string expected = @"Chain0:
action StartDialogue
if (party_has PartyMember.Sira) {
    if (get_switch Switch.Switch597) {
        if (get_switch Switch.Switch77) {
            map_text EventText.Sira2 11 StandardOptions
        } else {
            map_text EventText.Sira2 7 StandardOptions
        }
        map_text EventText.Sira2 8 ConversationOptions
        map_text EventText.Sira2 8 Conversation
    } else {
        map_text EventText.Sira2 7 StandardOptions
        map_text EventText.Sira2 1 ConversationOptions
        map_text EventText.Sira2 1 Conversation
    }
} else {
    if (get_switch Switch.Switch76) {
        map_text EventText.Sira2 16
    } else {
        map_text EventText.Sira2 15
    }
    switch Set Switch.Switch76
    if (prompt_player EventText.Sira2 17) {
        map_text EventText.Sira2 19
        add_party_member PartyMember.Sira 3
        if (result) {
            add_party_member PartyMember.Mellthas 3
            if (result) {
                npc_active 8 1 0 281
                npc_active 9 1 0 281
                end_dialogue
            } else {
                L1:
                map_text EventText.Sira2 18
                remove_party_member PartyMember.Sira 1 26888
                remove_party_member PartyMember.Mellthas 1 26889
            }
        } else {
            goto L1
        }
    } else {
        map_text EventText.Sira2 20
        end_dialogue
    }
}";
            TestDecompile(script, expected);
        }
    }
}
