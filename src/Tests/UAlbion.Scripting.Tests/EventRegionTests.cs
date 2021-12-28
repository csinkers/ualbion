using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using Xunit;

namespace UAlbion.Scripting.Tests;

[Event("s")]
public class TestStatementEvent : Event
{
    [EventPart("value")] public int Value { get; }
    public TestStatementEvent(int value) => Value = value;
}

[Event("b")]
public class TestBranchEvent : Event, IBranchingEvent
{
    [EventPart("value")] public int Value { get; }
    public TestBranchEvent(int value) => Value = value;
}

public class EventRegionTests
{
    public EventRegionTests() => Event.AddEventsFromAssembly(GetType().Assembly);

    static void Test(string script, string[] expected, string chainsString, string additionalEntryPointsString = null)
    {
        var events = EventNode.ParseRawEvents(script);
        var chains = chainsString.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(ushort.Parse);
        var additional = additionalEntryPointsString?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(ushort.Parse);
        var results = Decompiler.Decompile(events, chains, additional);

        var errors = new List<string>();
        if (expected.Length != results.Count)
            errors.Add($"Unexpected number of regions built: {results.Count} (expected {expected.Length})");

        for (int i = 0; i < results.Count && i < expected.Length; i++)
        {
            var visitor = new FormatScriptVisitor { PrettyPrint = false, WrapStatements = false };
            results[i].Accept(visitor);
            var compact = visitor.Code;

            visitor = new FormatScriptVisitor { PrettyPrint = true, WrapStatements = false };
            results[i].Accept(visitor);
            var pretty = visitor.Code;

            if (compact != expected[i] && pretty != expected[i])
                errors.Add($"Graph {i} was expected to be \"{expected[i]}\" but was \"{compact}\", or with full formatting:{Environment.NewLine}{pretty}");
        }

        if (!errors.Any()) 
            return;

        var combined = "Errors encountered:" + Environment.NewLine + string.Join(Environment.NewLine, errors);
        throw new InvalidOperationException(combined);
    }

    [Fact]
    public void SequenceRegionTest()
    {
        Test(@"0=>1: s 0
1=>2: s 1
2=>!: s 2", new[] { "Chain0:, s 0, s 1, s 2" }, "0");
    }

    [Fact]
    public void IfRegionTest()
    {
        Test(@"!0?1:2: b 0
1=>2: s 1
2=>!: s 2", new[] { "Chain0:, if (b 0) { s 1 }, s 2" }, "0");
    }

    [Fact]
    public void MultiChainRegionTest()
    {
        Test(@"0=>1: s 0
1=>2: s 1
2=>!: s 2
3=>4: s 3
4=>5: s 4
5=>!: s 5", new[]
        {
            "Chain0:, s 0, s 1, s 2",
            "Chain1:, s 3, s 4, s 5",
        }, "0 3");
    }

    [Fact]
    public void ExtraEntryRegionTest()
    {
        Test(@"0=>1: s 0
1=>2: s 1
2=>!: s 2
3=>4: s 3
4=>5: s 4
5=>!: s 5", new[]
        {
            "Chain0:, s 0, s 1, s 2",
            "Event3:, s 3, s 4, s 5",
        }, "0", "3");
    }

    [Fact]
    public void OverlappingChainsTest()
    {
        Test(@"0=>1: s 0
1=>2: s 1
2=>3: s 2
3=>4: s 3
4=>5: s 4
5=>!: s 5", new[]
        {
            @"Chain0:
s 0
s 1
s 2
s 3
s 4
s 5",

            @"Chain1:
s 3
s 4
s 5" }, "0 3");
    }

    [Fact]
    public void TwoChainsSharingASuffixRegionTest() // Ugh.. wish this didn't happen in the original game events. Oh well.
    {
        Test(@"0=>1: s 0
1=>2: s 1
2=>4: s 2
3=>4: s 3
4=>5: s 4
5=>!: s 5", new[]
        {
            @"Chain0:
s 0
s 1
s 2
s 4
s 5",

            @"Chain1:
s 3
s 4
s 5", }, "0 3");
    }
}