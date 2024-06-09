using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Scripting;
using UAlbion.Scripting.Ast;
using UAlbion.Scripting.Tests;
using UAlbion.TestCommon;
using Xunit;
using Xunit.Sdk;

namespace UAlbion.Base.Tests;

public class TestFixture : IDisposable
{
    public TestFixture()
    {
        Console.WriteLine("Start tests");
    }

    public void Dispose()
    {
        // For collecting stats etc over the whole test run
        // Console.WriteLine("CFG stats:");
        // foreach (var stat in Enum.GetValues<ControlFlowGraph.Stat>())
        //     Console.WriteLine($"    {stat}: {ControlFlowGraph.Stats[(int)stat]}");
        Console.WriteLine("End tests");
    }
}

[CollectionDefinition("DecompTests")]
public class DummyForTestFixture : ICollectionFixture<TestFixture> { }

[Collection("DecompTests")]
public class FullDecompilationTests : IDisposable
{
    static readonly string ResultsDir = Path.Combine(TestUtil.FindBasePath(), "re", "FullDecomp");
    static readonly IJsonUtil JsonUtil = new FormatJsonUtil();
    static readonly AssetMapping Mapping = new();
    static readonly EventExchange Exchange;
    static int _nextTestNum;

    readonly int _testNum;

    static FullDecompilationTests()
    {
        Event.AddEventsFromAssembly(typeof(ActionEvent).Assembly);
        AssetMapping.GlobalIsThreadLocal = true;
        var disk = new MockFileSystem(true);
        var baseDir = ConfigUtil.FindBasePath(disk);
        Exchange = AssetSystem.Setup(baseDir, "ualbion-tests", Mapping, disk, JsonUtil, null);
    }

    public FullDecompilationTests()
    {
        if (AssetMapping.Global.IsEmpty)
            AssetMapping.Global.MergeFrom(Mapping);

        _testNum = Interlocked.Increment(ref _nextTestNum);
        PerfTracker.StartupEvent($"Start decompilation test {_testNum}");
    }

    public void Dispose() => PerfTracker.StartupEvent($"Finish decompilation test {_testNum}");

    [Fact] public void EventSet1() => TestEventSet(new EventSetId(1));
    [Fact] public void EventSet100() => TestEventSet(new EventSetId(100));
    [Fact] public void EventSet101() => TestEventSet(new EventSetId(101));
    [Fact] public void EventSet102() => TestEventSet(new EventSetId(102));
    [Fact] public void EventSet103() => TestEventSet(new EventSetId(103));
    [Fact] public void EventSet104() => TestEventSet(new EventSetId(104));
    [Fact] public void EventSet105() => TestEventSet(new EventSetId(105));
    [Fact] public void EventSet106() => TestEventSet(new EventSetId(106));
    [Fact] public void EventSet107() => TestEventSet(new EventSetId(107));
    [Fact] public void EventSet108() => TestEventSet(new EventSetId(108));
    [Fact] public void EventSet109() => TestEventSet(new EventSetId(109));
    [Fact] public void EventSet110() => TestEventSet(new EventSetId(110));
    [Fact] public void EventSet111() => TestEventSet(new EventSetId(111));
    [Fact] public void EventSet112() => TestEventSet(new EventSetId(112));
    [Fact] public void EventSet113() => TestEventSet(new EventSetId(113));
    [Fact] public void EventSet114() => TestEventSet(new EventSetId(114));
    [Fact] public void EventSet115() => TestEventSet(new EventSetId(115));
    [Fact] public void EventSet116() => TestEventSet(new EventSetId(116));
    [Fact] public void EventSet117() => TestEventSet(new EventSetId(117));
    [Fact] public void EventSet118() => TestEventSet(new EventSetId(118));
    [Fact] public void EventSet119() => TestEventSet(new EventSetId(119));
    [Fact] public void EventSet120() => TestEventSet(new EventSetId(120));
    [Fact] public void EventSet121() => TestEventSet(new EventSetId(121));
    [Fact] public void EventSet122() => TestEventSet(new EventSetId(122));
    [Fact] public void EventSet123() => TestEventSet(new EventSetId(123));
    [Fact] public void EventSet124() => TestEventSet(new EventSetId(124));
    [Fact] public void EventSet125() => TestEventSet(new EventSetId(125));
    [Fact] public void EventSet126() => TestEventSet(new EventSetId(126));
    [Fact] public void EventSet127() => TestEventSet(new EventSetId(127));
    [Fact] public void EventSet128() => TestEventSet(new EventSetId(128));
    [Fact] public void EventSet129() => TestEventSet(new EventSetId(129));
    [Fact] public void EventSet130() => TestEventSet(new EventSetId(130));
    [Fact] public void EventSet131() => TestEventSet(new EventSetId(131));
    [Fact] public void EventSet132() => TestEventSet(new EventSetId(132));
    [Fact] public void EventSet133() => TestEventSet(new EventSetId(133));
    [Fact] public void EventSet134() => TestEventSet(new EventSetId(134));
    [Fact] public void EventSet135() => TestEventSet(new EventSetId(135));
    [Fact] public void EventSet136() => TestEventSet(new EventSetId(136));
    [Fact] public void EventSet137() => TestEventSet(new EventSetId(137));
    [Fact] public void EventSet138() => TestEventSet(new EventSetId(138));
    [Fact] public void EventSet139() => TestEventSet(new EventSetId(139));
    [Fact] public void EventSet140() => TestEventSet(new EventSetId(140));
    [Fact] public void EventSet141() => TestEventSet(new EventSetId(141));
    [Fact] public void EventSet142() => TestEventSet(new EventSetId(142));
    [Fact] public void EventSet143() => TestEventSet(new EventSetId(143));
    [Fact] public void EventSet144() => TestEventSet(new EventSetId(144));
    [Fact] public void EventSet145() => TestEventSet(new EventSetId(145));
    [Fact] public void EventSet146() => TestEventSet(new EventSetId(146));
    [Fact] public void EventSet147() => TestEventSet(new EventSetId(147));
    [Fact] public void EventSet148() => TestEventSet(new EventSetId(148));
    [Fact] public void EventSet149() => TestEventSet(new EventSetId(149));
    [Fact] public void EventSet150() => TestEventSet(new EventSetId(150));
    [Fact] public void EventSet151() => TestEventSet(new EventSetId(151));
    [Fact] public void EventSet152() => TestEventSet(new EventSetId(152));
    [Fact] public void EventSet153() => TestEventSet(new EventSetId(153));
    [Fact] public void EventSet154() => TestEventSet(new EventSetId(154));
    [Fact] public void EventSet155() => TestEventSet(new EventSetId(155));
    [Fact] public void EventSet156() => TestEventSet(new EventSetId(156));
    [Fact] public void EventSet157() => TestEventSet(new EventSetId(157));
    [Fact] public void EventSet158() => TestEventSet(new EventSetId(158));
    [Fact] public void EventSet159() => TestEventSet(new EventSetId(159));
    [Fact] public void EventSet160() => TestEventSet(new EventSetId(160));
    [Fact] public void EventSet161() => TestEventSet(new EventSetId(161));
    [Fact] public void EventSet162() => TestEventSet(new EventSetId(162));
    [Fact] public void EventSet163() => TestEventSet(new EventSetId(163));
    [Fact] public void EventSet164() => TestEventSet(new EventSetId(164));
    [Fact] public void EventSet165() => TestEventSet(new EventSetId(165));
    [Fact] public void EventSet166() => TestEventSet(new EventSetId(166));
    [Fact] public void EventSet167() => TestEventSet(new EventSetId(167));
    [Fact] public void EventSet168() => TestEventSet(new EventSetId(168));
    [Fact] public void EventSet169() => TestEventSet(new EventSetId(169));
    [Fact] public void EventSet170() => TestEventSet(new EventSetId(170));
    [Fact] public void EventSet171() => TestEventSet(new EventSetId(171));
    [Fact] public void EventSet172() => TestEventSet(new EventSetId(172));
    [Fact] public void EventSet173() => TestEventSet(new EventSetId(173));
    [Fact] public void EventSet174() => TestEventSet(new EventSetId(174));
    [Fact] public void EventSet175() => TestEventSet(new EventSetId(175));
    [Fact] public void EventSet176() => TestEventSet(new EventSetId(176));
    [Fact] public void EventSet179() => TestEventSet(new EventSetId(179));
    [Fact] public void EventSet180() => TestEventSet(new EventSetId(180));
    [Fact] public void EventSet181() => TestEventSet(new EventSetId(181));
    [Fact] public void EventSet183() => TestEventSet(new EventSetId(183));
    [Fact] public void EventSet184() => TestEventSet(new EventSetId(184));
    [Fact] public void EventSet185() => TestEventSet(new EventSetId(185));
    [Fact] public void EventSet186() => TestEventSet(new EventSetId(186));
    [Fact] public void EventSet187() => TestEventSet(new EventSetId(187));
    [Fact] public void EventSet188() => TestEventSet(new EventSetId(188));
    [Fact] public void EventSet189() => TestEventSet(new EventSetId(189));
    [Fact] public void EventSet190() => TestEventSet(new EventSetId(190));
    [Fact] public void EventSet191() => TestEventSet(new EventSetId(191));
    [Fact] public void EventSet192() => TestEventSet(new EventSetId(192));
    [Fact] public void EventSet193() => TestEventSet(new EventSetId(193));
    [Fact] public void EventSet194() => TestEventSet(new EventSetId(194));
    [Fact] public void EventSet195() => TestEventSet(new EventSetId(195));
    [Fact] public void EventSet2() => TestEventSet(new EventSetId(2));
    [Fact] public void EventSet200() => TestEventSet(new EventSetId(200));
    [Fact] public void EventSet201() => TestEventSet(new EventSetId(201));
    [Fact] public void EventSet202() => TestEventSet(new EventSetId(202));
    [Fact] public void EventSet203() => TestEventSet(new EventSetId(203));
    [Fact] public void EventSet210() => TestEventSet(new EventSetId(210));
    [Fact] public void EventSet211() => TestEventSet(new EventSetId(211));
    [Fact] public void EventSet212() => TestEventSet(new EventSetId(212));
    [Fact] public void EventSet213() => TestEventSet(new EventSetId(213));
    [Fact] public void EventSet214() => TestEventSet(new EventSetId(214));
    [Fact] public void EventSet215() => TestEventSet(new EventSetId(215));
    [Fact] public void EventSet216() => TestEventSet(new EventSetId(216));
    [Fact] public void EventSet217() => TestEventSet(new EventSetId(217));
    [Fact] public void EventSet218() => TestEventSet(new EventSetId(218));
    [Fact] public void EventSet219() => TestEventSet(new EventSetId(219));
    [Fact] public void EventSet220() => TestEventSet(new EventSetId(220));
    [Fact] public void EventSet222() => TestEventSet(new EventSetId(222));
    [Fact] public void EventSet223() => TestEventSet(new EventSetId(223));
    [Fact] public void EventSet224() => TestEventSet(new EventSetId(224));
    [Fact] public void EventSet225() => TestEventSet(new EventSetId(225));
    [Fact] public void EventSet226() => TestEventSet(new EventSetId(226));
    [Fact] public void EventSet227() => TestEventSet(new EventSetId(227));
    [Fact] public void EventSet228() => TestEventSet(new EventSetId(228));
    [Fact] public void EventSet229() => TestEventSet(new EventSetId(229));
    [Fact] public void EventSet230() => TestEventSet(new EventSetId(230));
    [Fact] public void EventSet231() => TestEventSet(new EventSetId(231));
    [Fact] public void EventSet233() => TestEventSet(new EventSetId(233));
    [Fact] public void EventSet234() => TestEventSet(new EventSetId(234));
    [Fact] public void EventSet235() => TestEventSet(new EventSetId(235));
    [Fact] public void EventSet236() => TestEventSet(new EventSetId(236));
    [Fact] public void EventSet237() => TestEventSet(new EventSetId(237));
    [Fact] public void EventSet238() => TestEventSet(new EventSetId(238));
    [Fact] public void EventSet239() => TestEventSet(new EventSetId(239));
    [Fact] public void EventSet240() => TestEventSet(new EventSetId(240));
    [Fact] public void EventSet241() => TestEventSet(new EventSetId(241));
    [Fact] public void EventSet242() => TestEventSet(new EventSetId(242));
    [Fact] public void EventSet243() => TestEventSet(new EventSetId(243));
    [Fact] public void EventSet244() => TestEventSet(new EventSetId(244));
    [Fact] public void EventSet245() => TestEventSet(new EventSetId(245));
    [Fact] public void EventSet246() => TestEventSet(new EventSetId(246));
    [Fact] public void EventSet247() => TestEventSet(new EventSetId(247));
    [Fact] public void EventSet248() => TestEventSet(new EventSetId(248));
    [Fact] public void EventSet249() => TestEventSet(new EventSetId(249));
    [Fact] public void EventSet250() => TestEventSet(new EventSetId(250));
    [Fact] public void EventSet251() => TestEventSet(new EventSetId(251));
    [Fact] public void EventSet252() => TestEventSet(new EventSetId(252));
    [Fact] public void EventSet253() => TestEventSet(new EventSetId(253));
    [Fact] public void EventSet255() => TestEventSet(new EventSetId(255));
    [Fact] public void EventSet256() => TestEventSet(new EventSetId(256));
    [Fact] public void EventSet257() => TestEventSet(new EventSetId(257));
    [Fact] public void EventSet258() => TestEventSet(new EventSetId(258));
    [Fact] public void EventSet259() => TestEventSet(new EventSetId(259));
    [Fact] public void EventSet260() => TestEventSet(new EventSetId(260));
    [Fact] public void EventSet261() => TestEventSet(new EventSetId(261));
    [Fact] public void EventSet262() => TestEventSet(new EventSetId(262));
    [Fact] public void EventSet263() => TestEventSet(new EventSetId(263));
    [Fact] public void EventSet264() => TestEventSet(new EventSetId(264));
    [Fact] public void EventSet266() => TestEventSet(new EventSetId(266));
    [Fact] public void EventSet267() => TestEventSet(new EventSetId(267));
    [Fact] public void EventSet268() => TestEventSet(new EventSetId(268));
    [Fact] public void EventSet269() => TestEventSet(new EventSetId(269));
    [Fact] public void EventSet270() => TestEventSet(new EventSetId(270));
    [Fact] public void EventSet271() => TestEventSet(new EventSetId(271));
    [Fact] public void EventSet272() => TestEventSet(new EventSetId(272));
    [Fact] public void EventSet273() => TestEventSet(new EventSetId(273));
    [Fact] public void EventSet274() => TestEventSet(new EventSetId(274));
    [Fact] public void EventSet275() => TestEventSet(new EventSetId(275));
    [Fact] public void EventSet276() => TestEventSet(new EventSetId(276));
    [Fact] public void EventSet277() => TestEventSet(new EventSetId(277));
    [Fact] public void EventSet278() => TestEventSet(new EventSetId(278));
    [Fact] public void EventSet279() => TestEventSet(new EventSetId(279));
    [Fact] public void EventSet280() => TestEventSet(new EventSetId(280));
    [Fact] public void EventSet281() => TestEventSet(new EventSetId(281));
    [Fact] public void EventSet282() => TestEventSet(new EventSetId(282));
    [Fact] public void EventSet283() => TestEventSet(new EventSetId(283));
    [Fact] public void EventSet284() => TestEventSet(new EventSetId(284));
    [Fact] public void EventSet285() => TestEventSet(new EventSetId(285));
    [Fact] public void EventSet286() => TestEventSet(new EventSetId(286));
    [Fact] public void EventSet287() => TestEventSet(new EventSetId(287));
    [Fact] public void EventSet288() => TestEventSet(new EventSetId(288));
    [Fact] public void EventSet289() => TestEventSet(new EventSetId(289));
    [Fact] public void EventSet290() => TestEventSet(new EventSetId(290));
    [Fact] public void EventSet291() => TestEventSet(new EventSetId(291));
    [Fact] public void EventSet292() => TestEventSet(new EventSetId(292));
    [Fact] public void EventSet293() => TestEventSet(new EventSetId(293));
    [Fact] public void EventSet294() => TestEventSet(new EventSetId(294));
    [Fact] public void EventSet295() => TestEventSet(new EventSetId(295));
    [Fact] public void EventSet296() => TestEventSet(new EventSetId(296));
    [Fact] public void EventSet3() => TestEventSet(new EventSetId(3));
    [Fact] public void EventSet301() => TestEventSet(new EventSetId(301));
    [Fact] public void EventSet302() => TestEventSet(new EventSetId(302));
    [Fact] public void EventSet303() => TestEventSet(new EventSetId(303));
    [Fact] public void EventSet304() => TestEventSet(new EventSetId(304));
    [Fact] public void EventSet305() => TestEventSet(new EventSetId(305));
    [Fact] public void EventSet306() => TestEventSet(new EventSetId(306));
    [Fact] public void EventSet307() => TestEventSet(new EventSetId(307));
    [Fact] public void EventSet308() => TestEventSet(new EventSetId(308));
    [Fact] public void EventSet311() => TestEventSet(new EventSetId(311));
    [Fact] public void EventSet312() => TestEventSet(new EventSetId(312));
    [Fact] public void EventSet314() => TestEventSet(new EventSetId(314));
    [Fact] public void EventSet315() => TestEventSet(new EventSetId(315));
    [Fact] public void EventSet316() => TestEventSet(new EventSetId(316));
    [Fact] public void EventSet317() => TestEventSet(new EventSetId(317));
    [Fact] public void EventSet318() => TestEventSet(new EventSetId(318));
    [Fact] public void EventSet319() => TestEventSet(new EventSetId(319));
    [Fact] public void EventSet320() => TestEventSet(new EventSetId(320));
    [Fact] public void EventSet321() => TestEventSet(new EventSetId(321));
    [Fact] public void EventSet322() => TestEventSet(new EventSetId(322));
    [Fact] public void EventSet323() => TestEventSet(new EventSetId(323));
    [Fact] public void EventSet324() => TestEventSet(new EventSetId(324));
    [Fact] public void EventSet981() => TestEventSet(new EventSetId(981));
    [Fact] public void EventSet982() => TestEventSet(new EventSetId(982));
    [Fact] public void EventSet983() => TestEventSet(new EventSetId(983));
    [Fact] public void EventSet984() => TestEventSet(new EventSetId(984));
    [Fact] public void EventSet985() => TestEventSet(new EventSetId(985));
    [Fact] public void EventSet986() => TestEventSet(new EventSetId(986));
    [Fact] public void EventSet987() => TestEventSet(new EventSetId(987));
    [Fact] public void EventSet989() => TestEventSet(new EventSetId(989));
    [Fact] public void EventSet990() => TestEventSet(new EventSetId(990));
    [Fact] public void EventSet999() => TestEventSet(new EventSetId(999));

    [Fact] public void Map100() => TestMap(new MapId(100));
    [Fact] public void Map101() => TestMap(new MapId(101));
    [Fact] public void Map102() => TestMap(new MapId(102));
    [Fact] public void Map103() => TestMap(new MapId(103));
    [Fact] public void Map104() => TestMap(new MapId(104));
    [Fact] public void Map105() => TestMap(new MapId(105));
    [Fact] public void Map106() => TestMap(new MapId(106));
    [Fact] public void Map107() => TestMap(new MapId(107));
    [Fact] public void Map108() => TestMap(new MapId(108));
    [Fact] public void Map109() => TestMap(new MapId(109));
    [Fact] public void Map110() => TestMap(new MapId(110));
    [Fact] public void Map111() => TestMap(new MapId(111));
    [Fact] public void Map112() => TestMap(new MapId(112));
    [Fact] public void Map113() => TestMap(new MapId(113));
    [Fact] public void Map114() => TestMap(new MapId(114));
    [Fact] public void Map115() => TestMap(new MapId(115));
    [Fact] public void Map116() => TestMap(new MapId(116));
    [Fact] public void Map117() => TestMap(new MapId(117));
    [Fact] public void Map118() => TestMap(new MapId(118));
    [Fact] public void Map119() => TestMap(new MapId(119));
    [Fact] public void Map120() => TestMap(new MapId(120));
    [Fact] public void Map121() => TestMap(new MapId(121));
    [Fact] public void Map122() => TestMap(new MapId(122));
    [Fact] public void Map123() => TestMap(new MapId(123));
    [Fact] public void Map124() => TestMap(new MapId(124));
    [Fact] public void Map125() => TestMap(new MapId(125));
    [Fact] public void Map126() => TestMap(new MapId(126));
    [Fact] public void Map127() => TestMap(new MapId(127));
    [Fact] public void Map128() => TestMap(new MapId(128));
    [Fact] public void Map129() => TestMap(new MapId(129));
    [Fact] public void Map130() => TestMap(new MapId(130));
    [Fact] public void Map131() => TestMap(new MapId(131));
    [Fact] public void Map132() => TestMap(new MapId(132));
    [Fact] public void Map133() => TestMap(new MapId(133));
    [Fact] public void Map134() => TestMap(new MapId(134));
    [Fact] public void Map135() => TestMap(new MapId(135));
    [Fact] public void Map136() => TestMap(new MapId(136));
    [Fact] public void Map137() => TestMap(new MapId(137));
    [Fact] public void Map138() => TestMap(new MapId(138));
    [Fact] public void Map139() => TestMap(new MapId(139));
    [Fact] public void Map140() => TestMap(new MapId(140));
    [Fact] public void Map141() => TestMap(new MapId(141));
    [Fact] public void Map142() => TestMap(new MapId(142));
    [Fact] public void Map143() => TestMap(new MapId(143));
    [Fact] public void Map144() => TestMap(new MapId(144));
    [Fact] public void Map145() => TestMap(new MapId(145));
    [Fact] public void Map146() => TestMap(new MapId(146));
    [Fact] public void Map147() => TestMap(new MapId(147));
    [Fact] public void Map148() => TestMap(new MapId(148));
    [Fact] public void Map149() => TestMap(new MapId(149));
    [Fact] public void Map150() => TestMap(new MapId(150));
    [Fact] public void Map151() => TestMap(new MapId(151));
    [Fact] public void Map152() => TestMap(new MapId(152));
    [Fact] public void Map153() => TestMap(new MapId(153));
    [Fact] public void Map154() => TestMap(new MapId(154));
    [Fact] public void Map155() => TestMap(new MapId(155));
    [Fact] public void Map156() => TestMap(new MapId(156));
    [Fact] public void Map157() => TestMap(new MapId(157));
    [Fact] public void Map158() => TestMap(new MapId(158));
    [Fact] public void Map159() => TestMap(new MapId(159));
    [Fact] public void Map160() => TestMap(new MapId(160));
    [Fact] public void Map161() => TestMap(new MapId(161));
    [Fact] public void Map162() => TestMap(new MapId(162));
    [Fact] public void Map163() => TestMap(new MapId(163));
    [Fact] public void Map164() => TestMap(new MapId(164));
    [Fact] public void Map165() => TestMap(new MapId(165));
    [Fact] public void Map166() => TestMap(new MapId(166));
    [Fact] public void Map167() => TestMap(new MapId(167));
    [Fact] public void Map168() => TestMap(new MapId(168));
    [Fact] public void Map169() => TestMap(new MapId(169));
    [Fact] public void Map170() => TestMap(new MapId(170));
    [Fact] public void Map171() => TestMap(new MapId(171));
    [Fact] public void Map172() => TestMap(new MapId(172));
    [Fact] public void Map173() => TestMap(new MapId(173));
    [Fact] public void Map174() => TestMap(new MapId(174));
    [Fact] public void Map190() => TestMap(new MapId(190));
    [Fact] public void Map195() => TestMap(new MapId(195));
    [Fact] public void Map196() => TestMap(new MapId(196));
    [Fact] public void Map197() => TestMap(new MapId(197));
    [Fact] public void Map198() => TestMap(new MapId(198));
    [Fact] public void Map199() => TestMap(new MapId(199));
    [Fact] public void Map200() => TestMap(new MapId(200));
    [Fact] public void Map201() => TestMap(new MapId(201));
    [Fact] public void Map202() => TestMap(new MapId(202));
    [Fact] public void Map203() => TestMap(new MapId(203));
    [Fact] public void Map204() => TestMap(new MapId(204));
    [Fact] public void Map205() => TestMap(new MapId(205));
    [Fact] public void Map206() => TestMap(new MapId(206));
    [Fact] public void Map207() => TestMap(new MapId(207));
    [Fact] public void Map210() => TestMap(new MapId(210));
    [Fact] public void Map211() => TestMap(new MapId(211));
    [Fact] public void Map212() => TestMap(new MapId(212));
    [Fact] public void Map213() => TestMap(new MapId(213));
    [Fact] public void Map214() => TestMap(new MapId(214));
    [Fact] public void Map215() => TestMap(new MapId(215));
    [Fact] public void Map216() => TestMap(new MapId(216));
    [Fact] public void Map217() => TestMap(new MapId(217));
    [Fact] public void Map218() => TestMap(new MapId(218));
    [Fact] public void Map219() => TestMap(new MapId(219));
    [Fact] public void Map230() => TestMap(new MapId(230));
    [Fact] public void Map231() => TestMap(new MapId(231));
    [Fact] public void Map232() => TestMap(new MapId(232));
    [Fact] public void Map233() => TestMap(new MapId(233));
    [Fact] public void Map234() => TestMap(new MapId(234));
    [Fact] public void Map235() => TestMap(new MapId(235));
    [Fact] public void Map236() => TestMap(new MapId(236));
    [Fact] public void Map237() => TestMap(new MapId(237));
    [Fact] public void Map238() => TestMap(new MapId(238));
    [Fact] public void Map239() => TestMap(new MapId(239));
    [Fact] public void Map240() => TestMap(new MapId(240));
    [Fact] public void Map241() => TestMap(new MapId(241));
    [Fact] public void Map242() => TestMap(new MapId(242));
    [Fact] public void Map243() => TestMap(new MapId(243));
    [Fact] public void Map244() => TestMap(new MapId(244));
    [Fact] public void Map245() => TestMap(new MapId(245));
    [Fact] public void Map246() => TestMap(new MapId(246));
    [Fact] public void Map247() => TestMap(new MapId(247));
    [Fact] public void Map248() => TestMap(new MapId(248));
    [Fact] public void Map249() => TestMap(new MapId(249));
    [Fact] public void Map250() => TestMap(new MapId(250));
    [Fact] public void Map251() => TestMap(new MapId(251));
    [Fact] public void Map252() => TestMap(new MapId(252));
    [Fact] public void Map253() => TestMap(new MapId(253));
    [Fact] public void Map254() => TestMap(new MapId(254));
    [Fact] public void Map255() => TestMap(new MapId(255));
    [Fact] public void Map256() => TestMap(new MapId(256));
    [Fact] public void Map260() => TestMap(new MapId(260));
    [Fact] public void Map261() => TestMap(new MapId(261));
    [Fact] public void Map262() => TestMap(new MapId(262));
    [Fact] public void Map263() => TestMap(new MapId(263));
    [Fact] public void Map264() => TestMap(new MapId(264));
    [Fact] public void Map265() => TestMap(new MapId(265));
    [Fact] public void Map266() => TestMap(new MapId(266));
    [Fact] public void Map267() => TestMap(new MapId(267));
    [Fact] public void Map268() => TestMap(new MapId(268));
    [Fact] public void Map269() => TestMap(new MapId(269));
    [Fact] public void Map270() => TestMap(new MapId(270));
    [Fact] public void Map271() => TestMap(new MapId(271));
    [Fact] public void Map272() => TestMap(new MapId(272));
    [Fact] public void Map273() => TestMap(new MapId(273));
    [Fact] public void Map274() => TestMap(new MapId(274));
    [Fact] public void Map275() => TestMap(new MapId(275));
    [Fact] public void Map276() => TestMap(new MapId(276));
    [Fact] public void Map277() => TestMap(new MapId(277));
    [Fact] public void Map278() => TestMap(new MapId(278));
    [Fact] public void Map279() => TestMap(new MapId(279));
    [Fact] public void Map280() => TestMap(new MapId(280));
    [Fact] public void Map281() => TestMap(new MapId(281));
    [Fact] public void Map282() => TestMap(new MapId(282));
    [Fact] public void Map283() => TestMap(new MapId(283));
    [Fact] public void Map284() => TestMap(new MapId(284));
    [Fact] public void Map290() => TestMap(new MapId(290));
    [Fact] public void Map291() => TestMap(new MapId(291));
    [Fact] public void Map292() => TestMap(new MapId(292));
    [Fact] public void Map293() => TestMap(new MapId(293));
    [Fact] public void Map294() => TestMap(new MapId(294));
    [Fact] public void Map295() => TestMap(new MapId(295));
    [Fact] public void Map297() => TestMap(new MapId(297));
    [Fact] public void Map298() => TestMap(new MapId(298));
    [Fact] public void Map299() => TestMap(new MapId(299));
    [Fact] public void Map300() => TestMap(new MapId(300));
    [Fact] public void Map301() => TestMap(new MapId(301));
    [Fact] public void Map302() => TestMap(new MapId(302));
    [Fact] public void Map303() => TestMap(new MapId(303));
    [Fact] public void Map304() => TestMap(new MapId(304));
    [Fact] public void Map305() => TestMap(new MapId(305));
    [Fact] public void Map310() => TestMap(new MapId(310));
    [Fact] public void Map311() => TestMap(new MapId(311));
    [Fact] public void Map312() => TestMap(new MapId(312));
    [Fact] public void Map313() => TestMap(new MapId(313));
    [Fact] public void Map320() => TestMap(new MapId(320));
    [Fact] public void Map322() => TestMap(new MapId(322));
    [Fact] public void Map388() => TestMap(new MapId(388));
    [Fact] public void Map389() => TestMap(new MapId(389));
    [Fact] public void Map390() => TestMap(new MapId(390));
    [Fact] public void Map398() => TestMap(new MapId(398));
    [Fact] public void Map399() => TestMap(new MapId(399));

    static void TestMap(MapId id, [CallerMemberName] string testName = null)
    {
        var map = Load(x => x.LoadMap(id));
        var npcRefs = map.Npcs.Where(x => x.Node != null).Select(x => x.Node.Id).ToHashSet();
        var zoneRefs = map.UniqueZoneNodeIds;
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

        if (set.Events.Count == 0)
            return;

        TestInner(set.Events, set.Chains, Array.Empty<ushort>(), testName);
    }

    static void TestInner<T>(
        IList<T> events,
        IEnumerable<ushort> chains,
        IEnumerable<ushort> entryPoints,
        [CallerMemberName] string testName = null) where T : IEventNode
    {
        var formatter = new EventFormatter(null, AssetId.None);
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
                var decompiledGraph = Decompile(graph, steps);
                var result = formatter.FormatGraphsAsBlocks(new[] { decompiledGraph }, 0);
                scripts[index] = result.Script;

                var roundTripLayout = AlbionCompiler.Compile(scripts[index], steps);
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

        int? dumpRegion = null;
        if (successCount < graphs.Count || dumpRegion.HasValue)
        {
            var combined = string.Join(Environment.NewLine, errors.Where(x => x.Length > 0));
            //*
            for (int i = 0; i < allSteps.Count; i++)
            {
                var steps = allSteps[i];
                if (!string.IsNullOrEmpty(errors[i]) || dumpRegion == i)
                    TestUtil.DumpSteps(steps, resultsDir, $"Region{i}");
            }
            //*/

            throw new InvalidOperationException($"[{successCount}/{graphs.Count}] Errors:{Environment.NewLine}{combined}");
        }
    }

    static ICfgNode Decompile(ControlFlowGraph graph, List<(string, IGraph)> steps)
    {
        var sw = Stopwatch.StartNew();
        ControlFlowGraph Record(string description, ControlFlowGraph g)
        {
            if (steps.Count == 0 || steps[^1].Item2 != g)
                steps.Add(($"{description} ({sw.ElapsedMilliseconds} ms)", g));
            sw.Restart();
            return g;
        }

        return Decompiler.SimplifyGraph(graph, Record);
    }

    static T Load<T>(Func<IAssetManager, T> func)
    {
        var assets = Exchange.Resolve<IAssetManager>();
        var result = func(assets);
        Assert.NotNull(result);

        return result;
    }
}
