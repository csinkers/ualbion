using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;
using UAlbion.Formats.MapEvents;
using UAlbion.Game;
using UAlbion.Game.Settings;
using UAlbion.Scripting;
using UAlbion.Scripting.Ast;
using UAlbion.Scripting.Tests;
using UAlbion.TestCommon;
using Xunit;
using Xunit.Sdk;

namespace UAlbion.Base.Tests
{
    public class FullDecompilationTests : IDisposable
    {
        static readonly string ResultsDir = Path.Combine(TestUtil.FindBasePath(), "re", "FullDecomp");
        static int s_testNum;

        static readonly IJsonUtil JsonUtil = new FormatJsonUtil();
        static readonly CoreConfig CoreConfig;
        static readonly GeneralConfig GeneralConfig;
        static readonly GameConfig GameConfig;
        static readonly GeneralSettings Settings;
        readonly int _testNum;

        static FullDecompilationTests()
        {
            var disk = new MockFileSystem(true);
            var baseDir = ConfigUtil.FindBasePath(disk);
            GeneralConfig = AssetSystem.LoadGeneralConfig(baseDir, disk, JsonUtil);
            CoreConfig = new CoreConfig();
            GameConfig = AssetSystem.LoadGameConfig(baseDir, disk, JsonUtil);
            Settings = new GeneralSettings
            {
                ActiveMods = { "Base" },
                Language = Language.English
            };
        }

        public FullDecompilationTests()
        {
            Event.AddEventsFromAssembly(typeof(ActionEvent).Assembly);
            AssetMapping.GlobalIsThreadLocal = true;
            AssetMapping.Global.Clear();
            _testNum = Interlocked.Increment(ref s_testNum);
            PerfTracker.StartupEvent($"Start decompilation test {_testNum}");
        }
        public void Dispose()
        {
            PerfTracker.StartupEvent($"Finish decompilation test {_testNum}");
        }

        [Fact] public void EventSet1() => TestEventSet(new EventSetId(AssetType.EventSet, 1));
        [Fact] public void EventSet100() => TestEventSet(new EventSetId(AssetType.EventSet, 100));
        [Fact] public void EventSet101() => TestEventSet(new EventSetId(AssetType.EventSet, 101));
        [Fact] public void EventSet102() => TestEventSet(new EventSetId(AssetType.EventSet, 102));
        [Fact] public void EventSet103() => TestEventSet(new EventSetId(AssetType.EventSet, 103));
        [Fact] public void EventSet104() => TestEventSet(new EventSetId(AssetType.EventSet, 104));
        [Fact] public void EventSet105() => TestEventSet(new EventSetId(AssetType.EventSet, 105));
        [Fact] public void EventSet106() => TestEventSet(new EventSetId(AssetType.EventSet, 106));
        [Fact] public void EventSet107() => TestEventSet(new EventSetId(AssetType.EventSet, 107));
        [Fact] public void EventSet108() => TestEventSet(new EventSetId(AssetType.EventSet, 108));
        [Fact] public void EventSet109() => TestEventSet(new EventSetId(AssetType.EventSet, 109));
        [Fact] public void EventSet110() => TestEventSet(new EventSetId(AssetType.EventSet, 110));
        [Fact] public void EventSet111() => TestEventSet(new EventSetId(AssetType.EventSet, 111));
        [Fact] public void EventSet112() => TestEventSet(new EventSetId(AssetType.EventSet, 112));
        [Fact] public void EventSet113() => TestEventSet(new EventSetId(AssetType.EventSet, 113));
        [Fact] public void EventSet114() => TestEventSet(new EventSetId(AssetType.EventSet, 114));
        [Fact] public void EventSet115() => TestEventSet(new EventSetId(AssetType.EventSet, 115));
        [Fact] public void EventSet116() => TestEventSet(new EventSetId(AssetType.EventSet, 116));
        [Fact] public void EventSet117() => TestEventSet(new EventSetId(AssetType.EventSet, 117));
        [Fact] public void EventSet118() => TestEventSet(new EventSetId(AssetType.EventSet, 118));
        [Fact] public void EventSet119() => TestEventSet(new EventSetId(AssetType.EventSet, 119));
        [Fact] public void EventSet120() => TestEventSet(new EventSetId(AssetType.EventSet, 120));
        [Fact] public void EventSet121() => TestEventSet(new EventSetId(AssetType.EventSet, 121));
        [Fact] public void EventSet122() => TestEventSet(new EventSetId(AssetType.EventSet, 122));
        [Fact] public void EventSet123() => TestEventSet(new EventSetId(AssetType.EventSet, 123));
        [Fact] public void EventSet124() => TestEventSet(new EventSetId(AssetType.EventSet, 124));
        [Fact] public void EventSet125() => TestEventSet(new EventSetId(AssetType.EventSet, 125));
        [Fact] public void EventSet126() => TestEventSet(new EventSetId(AssetType.EventSet, 126));
        [Fact] public void EventSet127() => TestEventSet(new EventSetId(AssetType.EventSet, 127));
        [Fact] public void EventSet128() => TestEventSet(new EventSetId(AssetType.EventSet, 128));
        [Fact] public void EventSet129() => TestEventSet(new EventSetId(AssetType.EventSet, 129));
        [Fact] public void EventSet130() => TestEventSet(new EventSetId(AssetType.EventSet, 130));
        [Fact] public void EventSet131() => TestEventSet(new EventSetId(AssetType.EventSet, 131));
        [Fact] public void EventSet132() => TestEventSet(new EventSetId(AssetType.EventSet, 132));
        [Fact] public void EventSet133() => TestEventSet(new EventSetId(AssetType.EventSet, 133));
        [Fact] public void EventSet134() => TestEventSet(new EventSetId(AssetType.EventSet, 134));
        [Fact] public void EventSet135() => TestEventSet(new EventSetId(AssetType.EventSet, 135));
        [Fact] public void EventSet136() => TestEventSet(new EventSetId(AssetType.EventSet, 136));
        [Fact] public void EventSet137() => TestEventSet(new EventSetId(AssetType.EventSet, 137));
        [Fact] public void EventSet138() => TestEventSet(new EventSetId(AssetType.EventSet, 138));
        [Fact] public void EventSet139() => TestEventSet(new EventSetId(AssetType.EventSet, 139));
        [Fact] public void EventSet140() => TestEventSet(new EventSetId(AssetType.EventSet, 140));
        [Fact] public void EventSet141() => TestEventSet(new EventSetId(AssetType.EventSet, 141));
        [Fact] public void EventSet142() => TestEventSet(new EventSetId(AssetType.EventSet, 142));
        [Fact] public void EventSet143() => TestEventSet(new EventSetId(AssetType.EventSet, 143));
        [Fact] public void EventSet144() => TestEventSet(new EventSetId(AssetType.EventSet, 144));
        [Fact] public void EventSet145() => TestEventSet(new EventSetId(AssetType.EventSet, 145));
        [Fact] public void EventSet146() => TestEventSet(new EventSetId(AssetType.EventSet, 146));
        [Fact] public void EventSet147() => TestEventSet(new EventSetId(AssetType.EventSet, 147));
        [Fact] public void EventSet148() => TestEventSet(new EventSetId(AssetType.EventSet, 148));
        [Fact] public void EventSet149() => TestEventSet(new EventSetId(AssetType.EventSet, 149));
        [Fact] public void EventSet150() => TestEventSet(new EventSetId(AssetType.EventSet, 150));
        [Fact] public void EventSet151() => TestEventSet(new EventSetId(AssetType.EventSet, 151));
        [Fact] public void EventSet152() => TestEventSet(new EventSetId(AssetType.EventSet, 152));
        [Fact] public void EventSet153() => TestEventSet(new EventSetId(AssetType.EventSet, 153));
        [Fact] public void EventSet154() => TestEventSet(new EventSetId(AssetType.EventSet, 154));
        [Fact] public void EventSet155() => TestEventSet(new EventSetId(AssetType.EventSet, 155));
        [Fact] public void EventSet156() => TestEventSet(new EventSetId(AssetType.EventSet, 156));
        [Fact] public void EventSet157() => TestEventSet(new EventSetId(AssetType.EventSet, 157));
        [Fact] public void EventSet158() => TestEventSet(new EventSetId(AssetType.EventSet, 158));
        [Fact] public void EventSet159() => TestEventSet(new EventSetId(AssetType.EventSet, 159));
        [Fact] public void EventSet160() => TestEventSet(new EventSetId(AssetType.EventSet, 160));
        [Fact] public void EventSet161() => TestEventSet(new EventSetId(AssetType.EventSet, 161));
        [Fact] public void EventSet162() => TestEventSet(new EventSetId(AssetType.EventSet, 162));
        [Fact] public void EventSet163() => TestEventSet(new EventSetId(AssetType.EventSet, 163));
        [Fact] public void EventSet164() => TestEventSet(new EventSetId(AssetType.EventSet, 164));
        [Fact] public void EventSet165() => TestEventSet(new EventSetId(AssetType.EventSet, 165));
        [Fact] public void EventSet166() => TestEventSet(new EventSetId(AssetType.EventSet, 166));
        [Fact] public void EventSet167() => TestEventSet(new EventSetId(AssetType.EventSet, 167));
        [Fact] public void EventSet168() => TestEventSet(new EventSetId(AssetType.EventSet, 168));
        [Fact] public void EventSet169() => TestEventSet(new EventSetId(AssetType.EventSet, 169));
        [Fact] public void EventSet170() => TestEventSet(new EventSetId(AssetType.EventSet, 170));
        [Fact] public void EventSet171() => TestEventSet(new EventSetId(AssetType.EventSet, 171));
        [Fact] public void EventSet172() => TestEventSet(new EventSetId(AssetType.EventSet, 172));
        [Fact] public void EventSet173() => TestEventSet(new EventSetId(AssetType.EventSet, 173));
        [Fact] public void EventSet174() => TestEventSet(new EventSetId(AssetType.EventSet, 174));
        [Fact] public void EventSet175() => TestEventSet(new EventSetId(AssetType.EventSet, 175));
        [Fact] public void EventSet176() => TestEventSet(new EventSetId(AssetType.EventSet, 176));
        [Fact] public void EventSet179() => TestEventSet(new EventSetId(AssetType.EventSet, 179));
        [Fact] public void EventSet180() => TestEventSet(new EventSetId(AssetType.EventSet, 180));
        [Fact] public void EventSet181() => TestEventSet(new EventSetId(AssetType.EventSet, 181));
        [Fact] public void EventSet183() => TestEventSet(new EventSetId(AssetType.EventSet, 183));
        [Fact] public void EventSet184() => TestEventSet(new EventSetId(AssetType.EventSet, 184));
        [Fact] public void EventSet185() => TestEventSet(new EventSetId(AssetType.EventSet, 185));
        [Fact] public void EventSet186() => TestEventSet(new EventSetId(AssetType.EventSet, 186));
        [Fact] public void EventSet187() => TestEventSet(new EventSetId(AssetType.EventSet, 187));
        [Fact] public void EventSet188() => TestEventSet(new EventSetId(AssetType.EventSet, 188));
        [Fact] public void EventSet189() => TestEventSet(new EventSetId(AssetType.EventSet, 189));
        [Fact] public void EventSet190() => TestEventSet(new EventSetId(AssetType.EventSet, 190));
        [Fact] public void EventSet191() => TestEventSet(new EventSetId(AssetType.EventSet, 191));
        [Fact] public void EventSet192() => TestEventSet(new EventSetId(AssetType.EventSet, 192));
        [Fact] public void EventSet193() => TestEventSet(new EventSetId(AssetType.EventSet, 193));
        [Fact] public void EventSet194() => TestEventSet(new EventSetId(AssetType.EventSet, 194));
        [Fact] public void EventSet195() => TestEventSet(new EventSetId(AssetType.EventSet, 195));
        [Fact] public void EventSet2() => TestEventSet(new EventSetId(AssetType.EventSet, 2));
        [Fact] public void EventSet200() => TestEventSet(new EventSetId(AssetType.EventSet, 200));
        [Fact] public void EventSet201() => TestEventSet(new EventSetId(AssetType.EventSet, 201));
        [Fact] public void EventSet202() => TestEventSet(new EventSetId(AssetType.EventSet, 202));
        [Fact] public void EventSet203() => TestEventSet(new EventSetId(AssetType.EventSet, 203));
        [Fact] public void EventSet210() => TestEventSet(new EventSetId(AssetType.EventSet, 210));
        [Fact] public void EventSet211() => TestEventSet(new EventSetId(AssetType.EventSet, 211));
        [Fact] public void EventSet212() => TestEventSet(new EventSetId(AssetType.EventSet, 212));
        [Fact] public void EventSet213() => TestEventSet(new EventSetId(AssetType.EventSet, 213));
        [Fact] public void EventSet214() => TestEventSet(new EventSetId(AssetType.EventSet, 214));
        [Fact] public void EventSet215() => TestEventSet(new EventSetId(AssetType.EventSet, 215));
        [Fact] public void EventSet216() => TestEventSet(new EventSetId(AssetType.EventSet, 216));
        [Fact] public void EventSet217() => TestEventSet(new EventSetId(AssetType.EventSet, 217));
        [Fact] public void EventSet218() => TestEventSet(new EventSetId(AssetType.EventSet, 218));
        [Fact] public void EventSet219() => TestEventSet(new EventSetId(AssetType.EventSet, 219));
        [Fact] public void EventSet220() => TestEventSet(new EventSetId(AssetType.EventSet, 220));
        [Fact] public void EventSet222() => TestEventSet(new EventSetId(AssetType.EventSet, 222));
        [Fact] public void EventSet223() => TestEventSet(new EventSetId(AssetType.EventSet, 223));
        [Fact] public void EventSet224() => TestEventSet(new EventSetId(AssetType.EventSet, 224));
        [Fact] public void EventSet225() => TestEventSet(new EventSetId(AssetType.EventSet, 225));
        [Fact] public void EventSet226() => TestEventSet(new EventSetId(AssetType.EventSet, 226));
        [Fact] public void EventSet227() => TestEventSet(new EventSetId(AssetType.EventSet, 227));
        [Fact] public void EventSet228() => TestEventSet(new EventSetId(AssetType.EventSet, 228));
        [Fact] public void EventSet229() => TestEventSet(new EventSetId(AssetType.EventSet, 229));
        [Fact] public void EventSet230() => TestEventSet(new EventSetId(AssetType.EventSet, 230));
        [Fact] public void EventSet231() => TestEventSet(new EventSetId(AssetType.EventSet, 231));
        [Fact] public void EventSet233() => TestEventSet(new EventSetId(AssetType.EventSet, 233));
        [Fact] public void EventSet234() => TestEventSet(new EventSetId(AssetType.EventSet, 234));
        [Fact] public void EventSet235() => TestEventSet(new EventSetId(AssetType.EventSet, 235));
        [Fact] public void EventSet236() => TestEventSet(new EventSetId(AssetType.EventSet, 236));
        [Fact] public void EventSet237() => TestEventSet(new EventSetId(AssetType.EventSet, 237));
        [Fact] public void EventSet238() => TestEventSet(new EventSetId(AssetType.EventSet, 238));
        [Fact] public void EventSet239() => TestEventSet(new EventSetId(AssetType.EventSet, 239));
        [Fact] public void EventSet240() => TestEventSet(new EventSetId(AssetType.EventSet, 240));
        [Fact] public void EventSet241() => TestEventSet(new EventSetId(AssetType.EventSet, 241));
        [Fact] public void EventSet242() => TestEventSet(new EventSetId(AssetType.EventSet, 242));
        [Fact] public void EventSet243() => TestEventSet(new EventSetId(AssetType.EventSet, 243));
        [Fact] public void EventSet244() => TestEventSet(new EventSetId(AssetType.EventSet, 244));
        [Fact] public void EventSet245() => TestEventSet(new EventSetId(AssetType.EventSet, 245));
        [Fact] public void EventSet246() => TestEventSet(new EventSetId(AssetType.EventSet, 246));
        [Fact] public void EventSet247() => TestEventSet(new EventSetId(AssetType.EventSet, 247));
        [Fact] public void EventSet248() => TestEventSet(new EventSetId(AssetType.EventSet, 248));
        [Fact] public void EventSet249() => TestEventSet(new EventSetId(AssetType.EventSet, 249));
        [Fact] public void EventSet250() => TestEventSet(new EventSetId(AssetType.EventSet, 250));
        [Fact] public void EventSet251() => TestEventSet(new EventSetId(AssetType.EventSet, 251));
        [Fact] public void EventSet252() => TestEventSet(new EventSetId(AssetType.EventSet, 252));
        [Fact] public void EventSet253() => TestEventSet(new EventSetId(AssetType.EventSet, 253));
        [Fact] public void EventSet255() => TestEventSet(new EventSetId(AssetType.EventSet, 255));
        [Fact] public void EventSet256() => TestEventSet(new EventSetId(AssetType.EventSet, 256));
        [Fact] public void EventSet257() => TestEventSet(new EventSetId(AssetType.EventSet, 257));
        [Fact] public void EventSet258() => TestEventSet(new EventSetId(AssetType.EventSet, 258));
        [Fact] public void EventSet259() => TestEventSet(new EventSetId(AssetType.EventSet, 259));
        [Fact] public void EventSet260() => TestEventSet(new EventSetId(AssetType.EventSet, 260));
        [Fact] public void EventSet261() => TestEventSet(new EventSetId(AssetType.EventSet, 261));
        [Fact] public void EventSet262() => TestEventSet(new EventSetId(AssetType.EventSet, 262));
        [Fact] public void EventSet263() => TestEventSet(new EventSetId(AssetType.EventSet, 263));
        [Fact] public void EventSet264() => TestEventSet(new EventSetId(AssetType.EventSet, 264));
        [Fact] public void EventSet266() => TestEventSet(new EventSetId(AssetType.EventSet, 266));
        [Fact] public void EventSet267() => TestEventSet(new EventSetId(AssetType.EventSet, 267));
        [Fact] public void EventSet268() => TestEventSet(new EventSetId(AssetType.EventSet, 268));
        [Fact] public void EventSet269() => TestEventSet(new EventSetId(AssetType.EventSet, 269));
        [Fact] public void EventSet270() => TestEventSet(new EventSetId(AssetType.EventSet, 270));
        [Fact] public void EventSet271() => TestEventSet(new EventSetId(AssetType.EventSet, 271));
        [Fact] public void EventSet272() => TestEventSet(new EventSetId(AssetType.EventSet, 272));
        [Fact] public void EventSet273() => TestEventSet(new EventSetId(AssetType.EventSet, 273));
        [Fact] public void EventSet274() => TestEventSet(new EventSetId(AssetType.EventSet, 274));
        [Fact] public void EventSet275() => TestEventSet(new EventSetId(AssetType.EventSet, 275));
        [Fact] public void EventSet276() => TestEventSet(new EventSetId(AssetType.EventSet, 276));
        [Fact] public void EventSet277() => TestEventSet(new EventSetId(AssetType.EventSet, 277));
        [Fact] public void EventSet278() => TestEventSet(new EventSetId(AssetType.EventSet, 278));
        [Fact] public void EventSet279() => TestEventSet(new EventSetId(AssetType.EventSet, 279));
        [Fact] public void EventSet280() => TestEventSet(new EventSetId(AssetType.EventSet, 280));
        [Fact] public void EventSet281() => TestEventSet(new EventSetId(AssetType.EventSet, 281));
        [Fact] public void EventSet282() => TestEventSet(new EventSetId(AssetType.EventSet, 282));
        [Fact] public void EventSet283() => TestEventSet(new EventSetId(AssetType.EventSet, 283));
        [Fact] public void EventSet284() => TestEventSet(new EventSetId(AssetType.EventSet, 284));
        [Fact] public void EventSet285() => TestEventSet(new EventSetId(AssetType.EventSet, 285));
        [Fact] public void EventSet286() => TestEventSet(new EventSetId(AssetType.EventSet, 286));
        [Fact] public void EventSet287() => TestEventSet(new EventSetId(AssetType.EventSet, 287));
        [Fact] public void EventSet288() => TestEventSet(new EventSetId(AssetType.EventSet, 288));
        [Fact] public void EventSet289() => TestEventSet(new EventSetId(AssetType.EventSet, 289));
        [Fact] public void EventSet290() => TestEventSet(new EventSetId(AssetType.EventSet, 290));
        [Fact] public void EventSet291() => TestEventSet(new EventSetId(AssetType.EventSet, 291));
        [Fact] public void EventSet292() => TestEventSet(new EventSetId(AssetType.EventSet, 292));
        [Fact] public void EventSet293() => TestEventSet(new EventSetId(AssetType.EventSet, 293));
        [Fact] public void EventSet294() => TestEventSet(new EventSetId(AssetType.EventSet, 294));
        [Fact] public void EventSet295() => TestEventSet(new EventSetId(AssetType.EventSet, 295));
        [Fact] public void EventSet296() => TestEventSet(new EventSetId(AssetType.EventSet, 296));
        [Fact] public void EventSet3() => TestEventSet(new EventSetId(AssetType.EventSet, 3));
        [Fact] public void EventSet301() => TestEventSet(new EventSetId(AssetType.EventSet, 301));
        [Fact] public void EventSet302() => TestEventSet(new EventSetId(AssetType.EventSet, 302));
        [Fact] public void EventSet303() => TestEventSet(new EventSetId(AssetType.EventSet, 303));
        [Fact] public void EventSet304() => TestEventSet(new EventSetId(AssetType.EventSet, 304));
        [Fact] public void EventSet305() => TestEventSet(new EventSetId(AssetType.EventSet, 305));
        [Fact] public void EventSet306() => TestEventSet(new EventSetId(AssetType.EventSet, 306));
        [Fact] public void EventSet307() => TestEventSet(new EventSetId(AssetType.EventSet, 307));
        [Fact] public void EventSet308() => TestEventSet(new EventSetId(AssetType.EventSet, 308));
        [Fact] public void EventSet311() => TestEventSet(new EventSetId(AssetType.EventSet, 311));
        [Fact] public void EventSet312() => TestEventSet(new EventSetId(AssetType.EventSet, 312));
        [Fact] public void EventSet314() => TestEventSet(new EventSetId(AssetType.EventSet, 314));
        [Fact] public void EventSet315() => TestEventSet(new EventSetId(AssetType.EventSet, 315));
        [Fact] public void EventSet316() => TestEventSet(new EventSetId(AssetType.EventSet, 316));
        [Fact] public void EventSet317() => TestEventSet(new EventSetId(AssetType.EventSet, 317));
        [Fact] public void EventSet318() => TestEventSet(new EventSetId(AssetType.EventSet, 318));
        [Fact] public void EventSet319() => TestEventSet(new EventSetId(AssetType.EventSet, 319));
        [Fact] public void EventSet320() => TestEventSet(new EventSetId(AssetType.EventSet, 320));
        [Fact] public void EventSet321() => TestEventSet(new EventSetId(AssetType.EventSet, 321));
        [Fact] public void EventSet322() => TestEventSet(new EventSetId(AssetType.EventSet, 322));
        [Fact] public void EventSet323() => TestEventSet(new EventSetId(AssetType.EventSet, 323));
        [Fact] public void EventSet324() => TestEventSet(new EventSetId(AssetType.EventSet, 324));
        [Fact] public void EventSet981() => TestEventSet(new EventSetId(AssetType.EventSet, 981));
        [Fact] public void EventSet982() => TestEventSet(new EventSetId(AssetType.EventSet, 982));
        [Fact] public void EventSet983() => TestEventSet(new EventSetId(AssetType.EventSet, 983));
        [Fact] public void EventSet984() => TestEventSet(new EventSetId(AssetType.EventSet, 984));
        [Fact] public void EventSet985() => TestEventSet(new EventSetId(AssetType.EventSet, 985));
        [Fact] public void EventSet986() => TestEventSet(new EventSetId(AssetType.EventSet, 986));
        [Fact] public void EventSet987() => TestEventSet(new EventSetId(AssetType.EventSet, 987));
        [Fact] public void EventSet989() => TestEventSet(new EventSetId(AssetType.EventSet, 989));
        [Fact] public void EventSet990() => TestEventSet(new EventSetId(AssetType.EventSet, 990));
        [Fact] public void EventSet999() => TestEventSet(new EventSetId(AssetType.EventSet, 999));

        [Fact] public void Map100() => TestMap(new MapId(AssetType.Map, 100));
        [Fact] public void Map101() => TestMap(new MapId(AssetType.Map, 101));
        [Fact] public void Map102() => TestMap(new MapId(AssetType.Map, 102));
        [Fact] public void Map103() => TestMap(new MapId(AssetType.Map, 103));
        [Fact] public void Map104() => TestMap(new MapId(AssetType.Map, 104));
        [Fact] public void Map105() => TestMap(new MapId(AssetType.Map, 105));
        [Fact] public void Map106() => TestMap(new MapId(AssetType.Map, 106));
        [Fact] public void Map107() => TestMap(new MapId(AssetType.Map, 107));
        [Fact] public void Map108() => TestMap(new MapId(AssetType.Map, 108));
        [Fact] public void Map109() => TestMap(new MapId(AssetType.Map, 109));
        [Fact] public void Map110() => TestMap(new MapId(AssetType.Map, 110));
        [Fact] public void Map111() => TestMap(new MapId(AssetType.Map, 111));
        [Fact] public void Map112() => TestMap(new MapId(AssetType.Map, 112));
        [Fact] public void Map113() => TestMap(new MapId(AssetType.Map, 113));
        [Fact] public void Map114() => TestMap(new MapId(AssetType.Map, 114));
        [Fact] public void Map115() => TestMap(new MapId(AssetType.Map, 115));
        [Fact] public void Map116() => TestMap(new MapId(AssetType.Map, 116));
        [Fact] public void Map117() => TestMap(new MapId(AssetType.Map, 117));
        [Fact] public void Map118() => TestMap(new MapId(AssetType.Map, 118));
        [Fact] public void Map119() => TestMap(new MapId(AssetType.Map, 119));
        [Fact] public void Map120() => TestMap(new MapId(AssetType.Map, 120));
        [Fact] public void Map121() => TestMap(new MapId(AssetType.Map, 121));
        [Fact] public void Map122() => TestMap(new MapId(AssetType.Map, 122));
        [Fact] public void Map123() => TestMap(new MapId(AssetType.Map, 123));
        [Fact] public void Map124() => TestMap(new MapId(AssetType.Map, 124));
        [Fact] public void Map125() => TestMap(new MapId(AssetType.Map, 125));
        [Fact] public void Map126() => TestMap(new MapId(AssetType.Map, 126));
        [Fact] public void Map127() => TestMap(new MapId(AssetType.Map, 127));
        [Fact] public void Map128() => TestMap(new MapId(AssetType.Map, 128));
        [Fact] public void Map129() => TestMap(new MapId(AssetType.Map, 129));
        [Fact] public void Map130() => TestMap(new MapId(AssetType.Map, 130));
        [Fact] public void Map131() => TestMap(new MapId(AssetType.Map, 131));
        [Fact] public void Map132() => TestMap(new MapId(AssetType.Map, 132));
        [Fact] public void Map133() => TestMap(new MapId(AssetType.Map, 133));
        [Fact] public void Map134() => TestMap(new MapId(AssetType.Map, 134));
        [Fact] public void Map135() => TestMap(new MapId(AssetType.Map, 135));
        [Fact] public void Map136() => TestMap(new MapId(AssetType.Map, 136));
        [Fact] public void Map137() => TestMap(new MapId(AssetType.Map, 137));
        [Fact] public void Map138() => TestMap(new MapId(AssetType.Map, 138));
        [Fact] public void Map139() => TestMap(new MapId(AssetType.Map, 139));
        [Fact] public void Map140() => TestMap(new MapId(AssetType.Map, 140));
        [Fact] public void Map141() => TestMap(new MapId(AssetType.Map, 141));
        [Fact] public void Map142() => TestMap(new MapId(AssetType.Map, 142));
        [Fact] public void Map143() => TestMap(new MapId(AssetType.Map, 143));
        [Fact] public void Map144() => TestMap(new MapId(AssetType.Map, 144));
        [Fact] public void Map145() => TestMap(new MapId(AssetType.Map, 145));
        [Fact] public void Map146() => TestMap(new MapId(AssetType.Map, 146));
        [Fact] public void Map147() => TestMap(new MapId(AssetType.Map, 147));
        [Fact] public void Map148() => TestMap(new MapId(AssetType.Map, 148));
        [Fact] public void Map149() => TestMap(new MapId(AssetType.Map, 149));
        [Fact] public void Map150() => TestMap(new MapId(AssetType.Map, 150));
        [Fact] public void Map151() => TestMap(new MapId(AssetType.Map, 151));
        [Fact] public void Map152() => TestMap(new MapId(AssetType.Map, 152));
        [Fact] public void Map153() => TestMap(new MapId(AssetType.Map, 153));
        [Fact] public void Map154() => TestMap(new MapId(AssetType.Map, 154));
        [Fact] public void Map155() => TestMap(new MapId(AssetType.Map, 155));
        [Fact] public void Map156() => TestMap(new MapId(AssetType.Map, 156));
        [Fact] public void Map157() => TestMap(new MapId(AssetType.Map, 157));
        [Fact] public void Map158() => TestMap(new MapId(AssetType.Map, 158));
        [Fact] public void Map159() => TestMap(new MapId(AssetType.Map, 159));
        [Fact] public void Map160() => TestMap(new MapId(AssetType.Map, 160));
        [Fact] public void Map161() => TestMap(new MapId(AssetType.Map, 161));
        [Fact] public void Map162() => TestMap(new MapId(AssetType.Map, 162));
        [Fact] public void Map163() => TestMap(new MapId(AssetType.Map, 163));
        [Fact] public void Map164() => TestMap(new MapId(AssetType.Map, 164));
        [Fact] public void Map165() => TestMap(new MapId(AssetType.Map, 165));
        [Fact] public void Map166() => TestMap(new MapId(AssetType.Map, 166));
        [Fact] public void Map167() => TestMap(new MapId(AssetType.Map, 167));
        [Fact] public void Map168() => TestMap(new MapId(AssetType.Map, 168));
        [Fact] public void Map169() => TestMap(new MapId(AssetType.Map, 169));
        [Fact] public void Map170() => TestMap(new MapId(AssetType.Map, 170));
        [Fact] public void Map171() => TestMap(new MapId(AssetType.Map, 171));
        [Fact] public void Map172() => TestMap(new MapId(AssetType.Map, 172));
        [Fact] public void Map173() => TestMap(new MapId(AssetType.Map, 173));
        [Fact] public void Map174() => TestMap(new MapId(AssetType.Map, 174));
        [Fact] public void Map190() => TestMap(new MapId(AssetType.Map, 190));
        [Fact] public void Map195() => TestMap(new MapId(AssetType.Map, 195));
        [Fact] public void Map196() => TestMap(new MapId(AssetType.Map, 196));
        [Fact] public void Map197() => TestMap(new MapId(AssetType.Map, 197));
        [Fact] public void Map198() => TestMap(new MapId(AssetType.Map, 198));
        [Fact] public void Map199() => TestMap(new MapId(AssetType.Map, 199));
        [Fact] public void Map200() => TestMap(new MapId(AssetType.Map, 200));
        [Fact] public void Map201() => TestMap(new MapId(AssetType.Map, 201));
        [Fact] public void Map202() => TestMap(new MapId(AssetType.Map, 202));
        [Fact] public void Map203() => TestMap(new MapId(AssetType.Map, 203));
        [Fact] public void Map204() => TestMap(new MapId(AssetType.Map, 204));
        [Fact] public void Map205() => TestMap(new MapId(AssetType.Map, 205));
        [Fact] public void Map206() => TestMap(new MapId(AssetType.Map, 206));
        [Fact] public void Map207() => TestMap(new MapId(AssetType.Map, 207));
        [Fact] public void Map210() => TestMap(new MapId(AssetType.Map, 210));
        [Fact] public void Map211() => TestMap(new MapId(AssetType.Map, 211));
        [Fact] public void Map212() => TestMap(new MapId(AssetType.Map, 212));
        [Fact] public void Map213() => TestMap(new MapId(AssetType.Map, 213));
        [Fact] public void Map214() => TestMap(new MapId(AssetType.Map, 214));
        [Fact] public void Map215() => TestMap(new MapId(AssetType.Map, 215));
        [Fact] public void Map216() => TestMap(new MapId(AssetType.Map, 216));
        [Fact] public void Map217() => TestMap(new MapId(AssetType.Map, 217));
        [Fact] public void Map218() => TestMap(new MapId(AssetType.Map, 218));
        [Fact] public void Map219() => TestMap(new MapId(AssetType.Map, 219));
        [Fact] public void Map230() => TestMap(new MapId(AssetType.Map, 230));
        [Fact] public void Map231() => TestMap(new MapId(AssetType.Map, 231));
        [Fact] public void Map232() => TestMap(new MapId(AssetType.Map, 232));
        [Fact] public void Map233() => TestMap(new MapId(AssetType.Map, 233));
        [Fact] public void Map234() => TestMap(new MapId(AssetType.Map, 234));
        [Fact] public void Map235() => TestMap(new MapId(AssetType.Map, 235));
        [Fact] public void Map236() => TestMap(new MapId(AssetType.Map, 236));
        [Fact] public void Map237() => TestMap(new MapId(AssetType.Map, 237));
        [Fact] public void Map238() => TestMap(new MapId(AssetType.Map, 238));
        [Fact] public void Map239() => TestMap(new MapId(AssetType.Map, 239));
        [Fact] public void Map240() => TestMap(new MapId(AssetType.Map, 240));
        [Fact] public void Map241() => TestMap(new MapId(AssetType.Map, 241));
        [Fact] public void Map242() => TestMap(new MapId(AssetType.Map, 242));
        [Fact] public void Map243() => TestMap(new MapId(AssetType.Map, 243));
        [Fact] public void Map244() => TestMap(new MapId(AssetType.Map, 244));
        [Fact] public void Map245() => TestMap(new MapId(AssetType.Map, 245));
        [Fact] public void Map246() => TestMap(new MapId(AssetType.Map, 246));
        [Fact] public void Map247() => TestMap(new MapId(AssetType.Map, 247));
        [Fact] public void Map248() => TestMap(new MapId(AssetType.Map, 248));
        [Fact] public void Map249() => TestMap(new MapId(AssetType.Map, 249));
        [Fact] public void Map250() => TestMap(new MapId(AssetType.Map, 250));
        [Fact] public void Map251() => TestMap(new MapId(AssetType.Map, 251));
        [Fact] public void Map252() => TestMap(new MapId(AssetType.Map, 252));
        [Fact] public void Map253() => TestMap(new MapId(AssetType.Map, 253));
        [Fact] public void Map254() => TestMap(new MapId(AssetType.Map, 254));
        [Fact] public void Map255() => TestMap(new MapId(AssetType.Map, 255));
        [Fact] public void Map256() => TestMap(new MapId(AssetType.Map, 256));
        [Fact] public void Map260() => TestMap(new MapId(AssetType.Map, 260));
        [Fact] public void Map261() => TestMap(new MapId(AssetType.Map, 261));
        [Fact] public void Map262() => TestMap(new MapId(AssetType.Map, 262));
        [Fact] public void Map263() => TestMap(new MapId(AssetType.Map, 263));
        [Fact] public void Map264() => TestMap(new MapId(AssetType.Map, 264));
        [Fact] public void Map265() => TestMap(new MapId(AssetType.Map, 265));
        [Fact] public void Map266() => TestMap(new MapId(AssetType.Map, 266));
        [Fact] public void Map267() => TestMap(new MapId(AssetType.Map, 267));
        [Fact] public void Map268() => TestMap(new MapId(AssetType.Map, 268));
        [Fact] public void Map269() => TestMap(new MapId(AssetType.Map, 269));
        [Fact] public void Map270() => TestMap(new MapId(AssetType.Map, 270));
        [Fact] public void Map271() => TestMap(new MapId(AssetType.Map, 271));
        [Fact] public void Map272() => TestMap(new MapId(AssetType.Map, 272));
        [Fact] public void Map273() => TestMap(new MapId(AssetType.Map, 273));
        [Fact] public void Map274() => TestMap(new MapId(AssetType.Map, 274));
        [Fact] public void Map275() => TestMap(new MapId(AssetType.Map, 275));
        [Fact] public void Map276() => TestMap(new MapId(AssetType.Map, 276));
        [Fact] public void Map277() => TestMap(new MapId(AssetType.Map, 277));
        [Fact] public void Map278() => TestMap(new MapId(AssetType.Map, 278));
        [Fact] public void Map279() => TestMap(new MapId(AssetType.Map, 279));
        [Fact] public void Map280() => TestMap(new MapId(AssetType.Map, 280));
        [Fact] public void Map281() => TestMap(new MapId(AssetType.Map, 281));
        [Fact] public void Map282() => TestMap(new MapId(AssetType.Map, 282));
        [Fact] public void Map283() => TestMap(new MapId(AssetType.Map, 283));
        [Fact] public void Map284() => TestMap(new MapId(AssetType.Map, 284));
        [Fact] public void Map290() => TestMap(new MapId(AssetType.Map, 290));
        [Fact] public void Map291() => TestMap(new MapId(AssetType.Map, 291));
        [Fact] public void Map292() => TestMap(new MapId(AssetType.Map, 292));
        [Fact] public void Map293() => TestMap(new MapId(AssetType.Map, 293));
        [Fact] public void Map294() => TestMap(new MapId(AssetType.Map, 294));
        [Fact] public void Map295() => TestMap(new MapId(AssetType.Map, 295));
        [Fact] public void Map297() => TestMap(new MapId(AssetType.Map, 297));
        [Fact] public void Map298() => TestMap(new MapId(AssetType.Map, 298));
        [Fact] public void Map299() => TestMap(new MapId(AssetType.Map, 299));
        [Fact] public void Map300() => TestMap(new MapId(AssetType.Map, 300));
        [Fact] public void Map301() => TestMap(new MapId(AssetType.Map, 301));
        [Fact] public void Map302() => TestMap(new MapId(AssetType.Map, 302));
        [Fact] public void Map303() => TestMap(new MapId(AssetType.Map, 303));
        [Fact] public void Map304() => TestMap(new MapId(AssetType.Map, 304));
        [Fact] public void Map305() => TestMap(new MapId(AssetType.Map, 305));
        [Fact] public void Map310() => TestMap(new MapId(AssetType.Map, 310));
        [Fact] public void Map311() => TestMap(new MapId(AssetType.Map, 311));
        [Fact] public void Map312() => TestMap(new MapId(AssetType.Map, 312));
        [Fact] public void Map313() => TestMap(new MapId(AssetType.Map, 313));
        [Fact] public void Map320() => TestMap(new MapId(AssetType.Map, 320));
        [Fact] public void Map322() => TestMap(new MapId(AssetType.Map, 322));
        [Fact] public void Map388() => TestMap(new MapId(AssetType.Map, 388));
        [Fact] public void Map389() => TestMap(new MapId(AssetType.Map, 389));
        [Fact] public void Map390() => TestMap(new MapId(AssetType.Map, 390));
        [Fact] public void Map398() => TestMap(new MapId(AssetType.Map, 398));
        [Fact] public void Map399() => TestMap(new MapId(AssetType.Map, 399));

        static void TestMap(MapId id, [CallerMemberName] string testName = null)
        {
            var map = Load(x => x.LoadMap(id));
            var npcRefs = map.Npcs.Where(x => x.Node != null).Select(x => x.Node.Id).ToHashSet();
            var zoneRefs = map.Zones.Where(x => x.Node != null).Select(x => x.Node.Id).ToHashSet();
            var refs = npcRefs.Union(zoneRefs).Except(map.Chains);

            TestInner(map.Events, map.Chains, refs, testName);
        }

        static void TestEventSet(EventSetId id, [CallerMemberName] string testName = null)
        {
            Formats.Assets.EventSet set;
            try
            {
                set = Load(x => x.LoadEventSet(id));
            }
            catch (NotNullException e) { throw new XunitException($"{e.Message} when loading {id}"); }

            if (set.Events.Length == 0)
                return;

            TestInner(set.Events, set.Chains, Array.Empty<ushort>(), testName);
        }

        static void TestInner<T>(
            IList<T> events,
            IEnumerable<ushort> chains,
            IEnumerable<ushort> entryPoints,
            [CallerMemberName] string testName = null) where T : IEventNode
        {
            var resultsDir = Path.Combine(ResultsDir, testName ?? "Unknown");
            var graphs = Decompiler.BuildEventRegions(events, chains, entryPoints);
            var scripts = new string[graphs.Count];
            var errors = new string[graphs.Count];
            var allSteps = new List<List<(string, IGraph)>>();
            int successCount = 0;

            for (var index = 0; index < graphs.Count; index++)
            {
                errors[index] = "";
                var steps = new List<(string, IGraph)>();
                allSteps.Add(steps);
                var graph = graphs[index];
                try
                {
                    var decompiled = Decompile(graph, steps);
                    var visitor = new FormatScriptVisitor();
                    decompiled.Accept(visitor);
                    scripts[index] = visitor.Code;

                    var roundTripLayout = ScriptCompiler.Compile(scripts[index], steps);
                    var expectedLayout = EventLayout.Build(new[] { graph });

                    if (!TestUtil.CompareLayout(roundTripLayout, expectedLayout, out var error))
                        errors[index] += $"[{index}: {error}] ";
                    else
                        successCount++;
                }
                catch (ControlFlowGraphException ex)
                {
                    steps.Add((ex.Message, ex.Graph));
                    errors[index] += $"[{index}: {ex.Message}] {ex.Graph.Defragment()} ";
                }
                catch (Exception ex)
                {
                    errors[index] += $"[{index}: {ex.Message}] ";
                }
            }

            if (successCount < graphs.Count)
            {
                var combined = string.Join(Environment.NewLine, errors.Where(x => x.Length > 0));
                //*
                for (int i = 0; i < allSteps.Count; i++)
                {
                    var steps = allSteps[i];
                    if (!string.IsNullOrEmpty(errors[i]))
                        TestUtil.DumpSteps(steps, resultsDir, $"Region{i}");
                }
                //*/

                throw new InvalidOperationException($"[{successCount}/{graphs.Count}] Errors:{Environment.NewLine}{combined}");
            }
        }

        static ICfgNode Decompile(ControlFlowGraph graph, List<(string, IGraph)> steps)
        {
            ControlFlowGraph Record(string description, ControlFlowGraph g)
            {
                if (steps.Count == 0 || steps[^1].Item2 != g)
                    steps.Add((description, g));
                return g;
            }

            return Decompiler.SimplifyGraph(graph, Record);
        }

        static T Load<T>(Func<IAssetManager, T> func)
        {
            var disk = new MockFileSystem(true);
            var exchange = AssetSystem.Setup(disk, JsonUtil, GeneralConfig, Settings, CoreConfig, GameConfig);

            var assets = exchange.Resolve<IAssetManager>();
            var result = func(assets);
            Assert.NotNull(result);

            return result;
        }
    }
}
