using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UAlbion.Api.Eventing;
using Xunit;

namespace UAlbion.Api.Tests;

[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
public class EventTests
{
    public EventTests()
    {
        Event.AddEventsFromAssembly(GetType().Assembly);
    }

    [Event("event0", "A basic event")]
    public class Event0 : Event { }

    [Fact]
    public void Event0Test()
    {
        var e = Event.Parse("event0", out _);
        Assert.IsType<Event0>(e);
        Assert.Equal("event0", e.ToString());
        Assert.IsType<Event0>(Event.Parse("EvEnT0", out _)); // Case insensitive
        Assert.Equal("event0", Event.Parse("EvEnT0", out _).ToString()); // Case insensitive

        Assert.Null(Event.Parse("some_nonsense_that_matches_nothing", out _));
        Assert.Null(Event.Parse(null, out _));
        Assert.Null(Event.Parse("", out _));
        Assert.Null(Event.Parse(" ", out _));
    }

    [Event("event1", "A simple event", "event_1")]
    public class Event1 : Event
    {
        public Event1(int id) => Id = id;

        [EventPart("id", "An identifier")] public int Id { get; }
    }

    [Fact]
    public void Event1Test()
    {
        var e = Event.Parse("event1 72", out _);
        Assert.IsType<Event1>(e);
        Assert.Equal(72, ((Event1) e).Id);
        Assert.Equal("event1 72", e.ToString());
    }

    [Fact]
    public void EventMetadataTest()
    {
        var meta = EventSerializer.Instance.GetEventMetadata().First(x => x.Type == typeof(Event1));
        Assert.Equal("event1", meta.Name);
        Assert.Equal(new[] { "event_1" }, meta.Aliases);
        Assert.Equal("A simple event", meta.HelpText);
        Assert.Collection(meta.Parts,
            partMeta =>
            {
                Assert.Equal("id", partMeta.Name);
                Assert.Equal("An identifier", partMeta.HelpText);
                Assert.Equal(typeof(int), partMeta.PropertyType);
                Assert.False(partMeta.IsOptional);
            });
    }

    public enum SomeEnum
    {
        First = 1,
        Second = 2,
    }

    [Event("enum_evt")]
    public class EnumEvent : Event
    {
        public EnumEvent(SomeEnum value) => Value = value;
        [EventPart("value")]
        public SomeEnum Value { get; } 
    }

    [Fact]
    public void EnumEventTest()
    {
        var e = Event.Parse("enum_evt First", out _);
        Assert.IsType<EnumEvent>(e);
        Assert.Equal(SomeEnum.First, ((EnumEvent)e).Value);
        Assert.Equal("enum_evt First", e.ToString());

        e = Event.Parse("enum_evt 1", out _);
        Assert.IsType<EnumEvent>(e);
        Assert.Equal(SomeEnum.First, ((EnumEvent)e).Value);

        e = Event.Parse("enum_evt 3", out _);
        Assert.IsType<EnumEvent>(e);
        Assert.Equal((SomeEnum)3, ((EnumEvent)e).Value);
        Assert.Equal("enum_evt 3", e.ToString());
    }

    [Event("string_evt")]
    public class StringEvent : Event
    {
        public StringEvent(string text) => Text = text;
        [EventPart("text")] public string Text { get; }
    }

    [Fact]
    public void StringEventTest()
    {
        var e = Event.Parse("string_evt simple_text", out _);
        Assert.IsType<StringEvent>(e);
        Assert.Equal("simple_text", ((StringEvent)e).Text);
        Assert.Equal("string_evt \"simple_text\"", e.ToString());

        e = Event.Parse("string_evt \"Some text\"", out _);
        Assert.IsType<StringEvent>(e);
        Assert.Equal("Some text", ((StringEvent)e).Text);
        Assert.Equal("string_evt \"Some text\"", e.ToString());

        e = Event.Parse("string_evt \"Some\\ttext\"", out _);
        Assert.IsType<StringEvent>(e);
        Assert.Equal("Some\ttext", ((StringEvent)e).Text);
        Assert.Equal("string_evt \"Some\\ttext\"", e.ToString());

        e = Event.Parse("string_evt \"Some \\\"text\\\"\"", out _);
        Assert.IsType<StringEvent>(e);
        Assert.Equal("Some \"text\"", ((StringEvent)e).Text);
        Assert.Equal("string_evt \"Some \\\"text\\\"\"", e.ToString());

        e = Event.Parse("string_evt \"Some \\\\slash\"", out _);
        Assert.IsType<StringEvent>(e);
        Assert.Equal("Some \\slash", ((StringEvent)e).Text);
        Assert.Equal("string_evt \"Some \\\\slash\"", e.ToString());
    }

    [Event("opt_evt")]
    public class OptionalEvent : Event
    {
        public OptionalEvent(int mandatory, int? optional = null)
        {
            Mandatory = mandatory;
            Optional = optional;
        }

        [EventPart("mandatory")]
        public int Mandatory { get; }
        [EventPart("optional", true)]
        public int? Optional { get; }
    }

    [Fact]
    public void OptionalEventTest()
    {
        var e = Event.Parse("opt_evt 1 2", out _);
        Assert.IsType<OptionalEvent>(e);
        Assert.Equal(1, ((OptionalEvent)e).Mandatory);
        Assert.Equal(2, ((OptionalEvent)e).Optional);
        Assert.Equal("opt_evt 1 2", e.ToString());

        e = Event.Parse("opt_evt 1", out _);
        Assert.IsType<OptionalEvent>(e);
        Assert.Equal(1, ((OptionalEvent)e).Mandatory);
        Assert.Null(((OptionalEvent)e).Optional);
        Assert.Equal("opt_evt 1", e.ToString());
    }

    [Event("custom_evt")]
    public class CustomParseEvent : Event
    {
        [EventPart("maybe")] public SomeEnum? MaybeEnum { get; }
        [EventPart("other")] public byte Other { get; }

        public static CustomParseEvent Parse(string[] parts)
        {
            int maybe = int.Parse(parts[1]);
            byte other = byte.Parse(parts[2]);
            return new CustomParseEvent(other, (SomeEnum?)maybe);
        }

        public CustomParseEvent(byte other, SomeEnum? maybe)
        {
            Other = other;
            MaybeEnum = maybe;
        }
    }

    [Fact]
    public void CustomParseEventTest()
    {
        var e = Event.Parse("custom_evt 1 12", out _);
        Assert.IsType<CustomParseEvent>(e);
        Assert.Equal(SomeEnum.First, ((CustomParseEvent)e).MaybeEnum);
        Assert.Equal(12, ((CustomParseEvent)e).Other);
        Assert.Equal("custom_evt First 12", e.ToString());

        Assert.Null(Event.Parse("custom_evt First 12", out _));
    }

    [Event("misc")]
    public class MiscEvent : Event
    {
        public MiscEvent(float f, bool? nullableBool = null, float? nullableFloat = null, SomeEnum? nullableEnum = null)
        {
            Float = f;
            NullableBool = nullableBool;
            NullableFloat = nullableFloat;
            NullableEnum = nullableEnum;
        }

        [EventPart("float")] public float Float { get; }
        [EventPart("nbool", true)] public bool? NullableBool { get; }
        [EventPart("nfloat", true)] public float? NullableFloat { get; }
        [EventPart("nenum", true)] public SomeEnum? NullableEnum { get; }
    }

    [Fact]
    public void MiscEventTest()
    {
        var e = Event.Parse("misc 1", out _);
        Assert.IsType<MiscEvent>(e);
        Assert.Equal(1.0f, ((MiscEvent)e).Float);
        Assert.Null(((MiscEvent)e).NullableBool);
        Assert.Null(((MiscEvent)e).NullableFloat);
        Assert.Null(((MiscEvent)e).NullableEnum);
        Assert.Equal("misc 1", e.ToString());

        e = Event.Parse("misc 1 true", out _);
        Assert.IsType<MiscEvent>(e);
        Assert.Equal(1.0f, ((MiscEvent)e).Float);
        Assert.Equal(true, ((MiscEvent)e).NullableBool);
        Assert.Null(((MiscEvent)e).NullableFloat);
        Assert.Null(((MiscEvent)e).NullableEnum);
        Assert.Equal("misc 1 True", e.ToString());

        e = Event.Parse("misc 1 True", out _);
        Assert.IsType<MiscEvent>(e);
        Assert.Equal(1.0f, ((MiscEvent)e).Float);
        Assert.Equal(true, ((MiscEvent)e).NullableBool);
        Assert.Null(((MiscEvent)e).NullableFloat);
        Assert.Null(((MiscEvent)e).NullableEnum);
        Assert.Equal("misc 1 True", e.ToString());

        e = Event.Parse("misc 1 false", out _);
        Assert.IsType<MiscEvent>(e);
        Assert.Equal(1.0f, ((MiscEvent)e).Float);
        Assert.Equal(false, ((MiscEvent)e).NullableBool);
        Assert.Null(((MiscEvent)e).NullableFloat);
        Assert.Null(((MiscEvent)e).NullableEnum);
        Assert.Equal("misc 1 False", e.ToString());

        e = Event.Parse("misc 1 True 3.1415", out _);
        Assert.IsType<MiscEvent>(e);
        Assert.Equal(1.0f, ((MiscEvent)e).Float);
        Assert.Equal(true, ((MiscEvent)e).NullableBool);
        Assert.NotNull(((MiscEvent)e).NullableFloat);
        Assert.True(Math.Abs((((MiscEvent)e).NullableFloat ?? 0) - 3.1415) < 1e-8);
        Assert.Null(((MiscEvent)e).NullableEnum);
        Assert.Equal("misc 1 True 3.1415", e.ToString());

        e = Event.Parse("misc 1 True 3.1415 Second", out _);
        Assert.IsType<MiscEvent>(e);
        Assert.Equal(1.0f, ((MiscEvent)e).Float);
        Assert.Equal(true, ((MiscEvent)e).NullableBool);
        Assert.NotNull(((MiscEvent)e).NullableFloat);
        Assert.True(Math.Abs((((MiscEvent)e).NullableFloat ?? 0) - 3.1415) < 1e-8);
        Assert.NotNull(((MiscEvent)e).NullableEnum);
        Assert.Equal(SomeEnum.Second, ((MiscEvent)e).NullableEnum);
        Assert.Equal("misc 1 True 3.1415 Second", e.ToString());
    }


    [Event("odd\\event")]
    public class OddEvent : Event { }

    [Fact]
    public void OddEventTest()
    {
        var e = Event.Parse(@"odd\event", out _);
        Assert.IsType<OddEvent>(e);
        Assert.Equal(@"odd\event", e.ToString());
    }

    public class NonSerializingEvent : Event {}

    [Fact]
    public void NonSerializingEventTest()
    {
        var e = new NonSerializingEvent();
        Assert.Equal("NonSerializingEvent", e.ToString());
    }
}