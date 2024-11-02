using System;
using System.Collections.Generic;
using Xunit;
using UAlbion.Api.Eventing;

namespace UAlbion.Api.Tests;

public class EventExchangeTests
{
    class TestComponent : IComponent
    {
        public List<IEvent> ReceivedEvents { get; } = new();

        public void Attach(EventExchange exchange) { }
        public void Remove() { }
        public void Receive(IEvent e, object sender) => ReceivedEvents.Add(e);

        public bool IsActive { get; set; }
        public int ComponentId => 1;
    }

    class TestQueryEvent : Event, IQueryEvent<bool> { }
    interface ISystem { } 
    class TestSystem : ISystem { }

    [Fact]
    public void TestSubscribe()
    {
        var exchange = new EventExchange();
        var component = new TestComponent();
        exchange.Subscribe<BasicEvent>(component);

        var recipients = exchange.EnumerateRecipients(typeof(BasicEvent));
        Assert.Contains(component, recipients);
    }

    [Fact]
    public void TestUnsubscribe()
    {
        var exchange = new EventExchange();
        var component = new TestComponent();
        exchange.Subscribe<BasicEvent>(component);
        exchange.Unsubscribe<BasicEvent>(component);

        var recipients = exchange.EnumerateRecipients(typeof(BasicEvent));
        Assert.DoesNotContain(component, recipients);
    }

    [Fact]
    public void TestRegisterAndResolve()
    {
        var exchange = new EventExchange();
        var system = new TestSystem();
        exchange.Register<ISystem>(system);

        var resolvedSystem = exchange.Resolve<ISystem>();
        Assert.Equal(system, resolvedSystem);
    }

    [Fact]
    public void TestUnregister()
    {
        var exchange = new EventExchange();
        var system = new TestSystem();
        exchange.Register(system);
        exchange.Unregister(system);

        var resolvedSystem = exchange.Resolve<TestSystem>();
        Assert.Null(resolvedSystem);
    }

    [Fact]
    public void TestRaise()
    {
        var exchange = new EventExchange();
        var component = new TestComponent();
        exchange.Subscribe<BasicEvent>(component);

        var testEvent = new BasicEvent();
        exchange.Raise(testEvent, this);

        Assert.Contains(component.ReceivedEvents, x => x == testEvent);
    }

    [Fact]
    public void TestRaiseAdHoc()
    {
        var exchange = new EventExchange();
        bool received = false;
        var component = new AdHocComponent("C1", x => x.On<BasicEvent>(_ => received = true));
        exchange.Attach(component);

        var testEvent = new BasicEvent();
        exchange.Raise(testEvent, this);

        Assert.True(received);
    }

    [Fact]
    public void TestRaiseQuery()
    {
        var exchange = new EventExchange();
        var trueComponent = new AdHocComponent("ReturnTrue", x => x.OnQuery<TestQueryEvent, bool>(_ => true));
        exchange.Attach(trueComponent);

        var testQueryEvent = new TestQueryEvent();
        var result = exchange.RaiseQuery(testQueryEvent, this);

        Assert.True(result);
    }

    [Fact]
    public void TestRaiseQuery2()
    {
        var exchange = new EventExchange();
        var falseComponent = new AdHocComponent("ReturnFalse", x => x.OnQuery<TestQueryEvent, bool>(_ => false));
        exchange.Attach(falseComponent);

        var testQueryEvent = new TestQueryEvent();
        var result = exchange.RaiseQuery(testQueryEvent, this);

        Assert.False(result);
    }

    [Fact]
    public void TestRaiseQuery_MultipleResults()
    {
        var exchange = new EventExchange();
        var trueComponent = new AdHocComponent("ReturnTrue", x => x.OnQuery<TestQueryEvent, bool>(_ => true));
        var falseComponent = new AdHocComponent("ReturnFalse", x => x.OnQuery<TestQueryEvent, bool>(_ => false));
        exchange.Attach(trueComponent);
        exchange.Attach(falseComponent);

        var testQueryEvent = new TestQueryEvent();
        Assert.Throws<InvalidOperationException>(() => exchange.RaiseQuery(testQueryEvent, this));
    }

    [Fact]
    public void TestRaiseQuery_NoResults()
    {
        var exchange = new EventExchange();
        var testQueryEvent = new TestQueryEvent();
        Assert.Throws<InvalidOperationException>(() => exchange.RaiseQuery(testQueryEvent, this));
    }

    [Fact]
    public void TestRaiseAsync_HandleSync()
    {
        var exchange = new EventExchange();
        bool received = false;
        var component = new AdHocComponent("C1", x => x.On<BasicEvent>(_ => received = true));
        exchange.Attach(component);

        var testEvent = new BasicEvent();
        var task = exchange.RaiseA(testEvent, this);

        Assert.True(task.IsCompleted);
        Assert.True(received);
    }

    [Fact]
    public void TestRaiseAsync_HandleAsync_Wait()
    {
        var exchange = new EventExchange();
        bool received = false;
        var atc = new AlbionTaskCore();
        var component = new AdHocComponent("C1", x => x.OnAsync<BasicEvent>(_ =>
        {
            received = true;
            return atc.UntypedTask;
        }));
        exchange.Attach(component);

        var testEvent = new BasicEvent();
        var task = exchange.RaiseA(testEvent, this);

        Assert.False(task.IsCompleted);
        Assert.True(received);

        atc.Complete();
        Assert.True(task.IsCompleted);
    }

    [Fact]
    public void TestRaiseAsync_HandleAsync_Wait2()
    {
        var exchange = new EventExchange();
        bool received1 = false;
        bool received2 = false;
        var atc1 = new AlbionTaskCore();
        var atc2 = new AlbionTaskCore();
        var component1 = new AdHocComponent("C1", x => x.OnAsync<BasicEvent>(_ =>
        {
            received1 = true;
            return atc1.UntypedTask;
        }));

        var component2 = new AdHocComponent("C2", x => x.OnAsync<BasicEvent>(_ =>
        {
            received2 = true;
            return atc2.UntypedTask;
        }));

        exchange.Attach(component1);
        exchange.Attach(component2);

        var testEvent = new BasicEvent();
        var task = exchange.RaiseA(testEvent, this);

        Assert.False(task.IsCompleted);
        Assert.True(received1);
        Assert.True(received2);

        atc1.Complete();
        Assert.False(task.IsCompleted);

        atc2.Complete();
        Assert.True(task.IsCompleted);
    }

    [Fact]
    public void TestRaiseAsync_HandleAsync_CompleteImmediately()
    {
        var exchange = new EventExchange();
        bool received = false;
        var component = new AdHocComponent("C1", x => x.OnAsync<BasicEvent>(_ =>
        {
            received = true;
            return AlbionTask.CompletedTask;
        }));

        exchange.Attach(component);

        var testEvent = new BasicEvent();
        var task = exchange.RaiseA(testEvent, this);

        Assert.True(received);
        Assert.True(task.IsCompleted);
    }

    [Fact]
    public void TestRaiseAsync_HandleAsync_CompleteImmediately2()
    {
        var exchange = new EventExchange();
        bool received1 = false;
        bool received2 = false;

        var component1 = new AdHocComponent("C1", x => x.OnAsync<BasicEvent>(_ => { received1 = true; return AlbionTask.CompletedTask; }));
        var component2 = new AdHocComponent("C2", x => x.On<BasicEvent>(_ => received2 = true));

        exchange.Attach(component1);
        exchange.Attach(component2);

        var testEvent = new BasicEvent();
        var task = exchange.RaiseA(testEvent, this);

        Assert.True(received1);
        Assert.True(received2);
        Assert.True(task.IsCompleted);
    }

    [Fact]
    public void TestRaiseAsync_HandleAsync_SomeWait()
    {
        var exchange = new EventExchange();
        bool received1 = false;
        var atc1 = new AlbionTaskCore();
        bool received2 = false;

        var component1 = new AdHocComponent("C1", x => x.OnAsync<BasicEvent>(_ => { received1 = true; return atc1.UntypedTask; }));
        var component2 = new AdHocComponent("C2", x => x.On<BasicEvent>(_ => received2 = true));

        exchange.Attach(component1);
        exchange.Attach(component2);

        var testEvent = new BasicEvent();
        var task = exchange.RaiseA(testEvent, this);

        Assert.True(received1);
        Assert.True(received2);
        Assert.False(task.IsCompleted);

        atc1.Complete();
        Assert.True(task.IsCompleted);
    }


    [Fact]
    public void TestRaiseAsyncQuery()
    {
        var exchange = new EventExchange();
        var trueComponent = new AdHocComponent("ReturnTrue", x => x.OnQueryAsync<TestQueryEvent, bool>(_ => AlbionTask.True));
        exchange.Attach(trueComponent);

        var testQueryEvent = new TestQueryEvent();
        var task = exchange.RaiseQueryA(testQueryEvent, this);

        Assert.True(task.IsCompleted);
        Assert.True(task.GetResult());
    }

    [Fact]
    public void TestRaiseAsyncQuery2()
    {
        var exchange = new EventExchange();
        var falseComponent = new AdHocComponent("ReturnFalse", x => x.OnQueryAsync<TestQueryEvent, bool>(_ => AlbionTask.False));
        exchange.Attach(falseComponent);

        var testQueryEvent = new TestQueryEvent();
        var task = exchange.RaiseQueryA(testQueryEvent, this);

        Assert.True(task.IsCompleted);
        Assert.False(task.GetResult());
    }

    [Fact]
    public void TestRaiseAsyncQuery3()
    {
        var exchange = new EventExchange();
        var atc = new AlbionTaskCore<bool>();
        var trueComponent = new AdHocComponent("ReturnTrue", x => x.OnQueryAsync<TestQueryEvent, bool>(_ => atc.Task));
        exchange.Attach(trueComponent);

        var testQueryEvent = new TestQueryEvent();
        var task = exchange.RaiseQueryA(testQueryEvent, this);

        Assert.False(task.IsCompleted);

        atc.SetResult(true);
        Assert.True(task.IsCompleted);
        Assert.True(task.GetResult());
    }

    [Fact]
    public void TestRaiseAsyncQuery_MultipleResults()
    {
        var exchange = new EventExchange();
        var trueComponent = new AdHocComponent("ReturnTrue", x => x.OnQueryAsync<TestQueryEvent, bool>(_ => AlbionTask.True));
        var falseComponent = new AdHocComponent("ReturnFalse", x => x.OnQueryAsync<TestQueryEvent, bool>(_ => AlbionTask.False));
        exchange.Attach(trueComponent);
        exchange.Attach(falseComponent);

        var testQueryEvent = new TestQueryEvent();
        Assert.Throws<InvalidOperationException>(() => exchange.RaiseQuery(testQueryEvent, this));
    }

    [Fact]
    public void TestRaiseAsyncQuery_NoResults()
    {
        var exchange = new EventExchange();
        var testQueryEvent = new TestQueryEvent();
        Assert.Throws<InvalidOperationException>(() => exchange.RaiseQuery(testQueryEvent, this));
    }
}
