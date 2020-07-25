using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UAlbion.Core.Tests
{
    public class ComponentTests
    {
        [Fact]
        public void BasicComponentTest()
        {
            var c = new BasicComponent();
            c.AddHandler<BasicEvent>(_ => { });
            var e = new BasicEvent();
            var x = new EventExchange(new BasicLogExchange());

            Assert.Equal(0, c.Handled);
            Assert.True(c.IsActive);

            c.Receive(e, this);
            Assert.Equal(0,c.Handled); // Handlers don't fire if component isn't attached

            c.Attach(x);
            c.Receive(e, this);
            Assert.Equal(1, c.Handled);

            c.Attach(x);
            c.Attach(x);
            c.Attach(x);
            c.Attach(x);
            c.Receive(e, this);
            Assert.Equal(2, c.Handled);

            c.Remove();
            c.Receive(e, this);
            Assert.Equal(2, c.Handled);

            c.Remove();
            c.Remove();
            c.Receive(e, this);
            Assert.Equal(2, c.Handled);
        }

        public interface IBasicInterface { }
        public class BasicImplementation : IBasicInterface { }

        [Fact]
        public void ResolveTest()
        {
            var c = new BasicComponent();
            var x = new EventExchange(new BasicLogExchange());
            x.Attach(c);

            Assert.Null(c.CallResolve<IBasicInterface>());

            var imp = new BasicImplementation();
            x.Register<IBasicInterface>(imp);

            Assert.Equal(imp, c.CallResolve<IBasicInterface>());

            x.Unregister(imp);
            Assert.Null(c.CallResolve<IBasicInterface>());
        }

        [Fact]
        public void RaiseTest()
        {
            var c1 = new BasicComponent();
            c1.AddHandler<BasicEvent>(_ => { });
            var c2 = new BasicComponent();
            c2.AddHandler<BasicEvent>(_ => { });
            var e = new BasicEvent();
            var ee = new EventExchange(new BasicLogExchange());

            ee.Attach(c1);
            ee.Attach(c2);

            Assert.Equal(0, c1.Handled);
            Assert.Equal(0, c2.Handled);

            c1.Raise(e);

            Assert.Equal(0, c1.Handled); // Components shouldn't see their own events.
            Assert.Equal(1, c2.Handled);

            var recipients = ee.EnumerateRecipients(typeof(BasicEvent)).ToList();
            Assert.Collection(recipients,
                x => Assert.Equal(x, c1),
                x => Assert.Equal(x, c2));
        }

        [Fact]
        public void RaiseAsyncTest()
        {
            var pending2 = new Queue<Action>();
            var pending3 = new Queue<Action<bool>>();
            var c1 = new BasicComponent();
            var c2 = new BasicComponent();
            var c3 = new BasicComponent();
            c1.AddHandler<BasicAsyncEvent>(_ => { });
            c2.AddAsyncHandler<BasicAsyncEvent>((_, c) => { pending2.Enqueue(c); return true; });
            c3.AddAsyncHandler<BasicAsyncEvent, bool>((_, c) => { pending3.Enqueue(c); return true; });

            void Check(int seen1, int seen2, int seen3, int handled1, int handled2, int handled3)
            {
                Assert.Equal(seen1, c1.Seen);
                Assert.Equal(seen2, c2.Seen);
                Assert.Equal(seen3, c3.Seen);
                Assert.Equal(handled1, c1.Handled);
                Assert.Equal(handled2, c2.Handled);
                Assert.Equal(handled3, c3.Handled);
            }

            var e = new BasicAsyncEvent();
            var ee = new EventExchange(new BasicLogExchange());

            ee.Attach(c1);
            ee.Attach(c2);
            ee.Attach(c3);
            Assert.Equal(0, c1.Seen + c2.Seen + c3.Seen);
            Assert.Equal(0, c1.Handled + c2.Handled + c3.Handled);

            ee.Raise(e, this);
            Check(1, 1, 1, 1, 0, 0);
            Assert.Collection(pending2, _ => { });
            Assert.Collection(pending3, _ => { });

            pending2.Dequeue()();
            Check(1, 1, 1, 1, 1, 0);

            pending3.Dequeue()(true);
            Check(1, 1, 1, 1, 1, 1);

            var recipients = ee.EnumerateRecipients(typeof(BasicAsyncEvent)).ToList();
            Assert.Collection(recipients,
                x => Assert.Equal(x, c1),
                x => Assert.Equal(x, c2),
                x => Assert.Equal(x, c3)
            );

            int total = 0;
            int totalTrue = 0;

            void BoolContinuation(bool x)
            {
                total++;
                if (x) totalTrue++;
            }

            void PlainContinuation()
            {
                total++;
            }

            Assert.Equal(2, ee.RaiseAsync<bool>(e, this, BoolContinuation)); // 2 async handlers, 1 sync
            Check(2, 2, 2, 2, 1, 1);
            pending2.Dequeue()(); // This attempted completion will not register as null cannot be coerced to bool.
            Check(2, 2, 2, 2, 2, 1);
            Assert.Equal(0, total);
            pending3.Dequeue()(true);
            Check(2, 2, 2, 2, 2, 2);
            Assert.Equal(1, total);
            Assert.Equal(1, totalTrue);

            Assert.Equal(1, c3.RaiseAsync(e, PlainContinuation)); // 2 async handlers, 1 sync but c3 is raising so shouldn't receive it.
            Check(3, 3, 2, 3, 2, 2);
            Assert.Empty(pending3);
            pending2.Dequeue()();
            Check(3, 3, 2, 3, 3, 2);
            Assert.Equal(2, total);

            Assert.Equal(2, c1.RaiseAsync(e, BoolContinuation)); // 2 async handlers, 1 sync but c1 raises.
            Check(3, 4, 3, 3, 3, 2);
            pending2.Dequeue()(); // This attempted completion will not register as null cannot be coerced to bool.
            Check(3, 4, 3, 3, 4, 2); 
            pending3.Dequeue()(true);
            Check(3, 4, 3, 3, 4, 3);
        }

        [Fact]
        public void DisableHandlerTest()
        {
            var c = new BasicComponent();
            var e = new BasicEvent();
            var x = new EventExchange(new BasicLogExchange());
            c.AddHandler<BasicEvent>(_ => { });

            c.Attach(x);
            Assert.Equal(0, c.Handled);

            c.Receive(e, this);
            Assert.Equal(1, c.Handled);

            c.AddHandler<BasicEvent>(_ => { });
            c.Receive(e, this);
            Assert.Equal(2, c.Handled);

            c.RemoveHandler<BasicEvent>();
            c.Receive(e, this);
            Assert.Equal(2, c.Handled);

            c.AddHandler<BasicEvent>(_ => { });
            c.Receive(e, this);
            Assert.Equal(3, c.Handled);

            c.AddHandler<BasicEvent>(_ => throw new InvalidOperationException()); // Registering a handler for an event that's already handled should be a no-op
            c.Receive(e, this);
            Assert.Equal(4, c.Handled);
        }

        [Fact]
        public void HandlerFormatTest()
        {
            var c = new BasicComponent();
            Assert.Equal("H<BasicComponent, BasicEvent>", new Handler<BasicEvent>(_ => { }, c).ToString());
        }

        [Fact]
        public void ChildTest()
        {
            var x = new EventExchange(new BasicLogExchange());
            var parent = new BasicComponent();
            var child1 = new BasicComponent();
            var child2 = new BasicComponent();

            parent.AddHandler<BasicEvent>(_ => { });
            child1.AddHandler<BasicEvent>(_ => { });
            child2.AddHandler<BasicEvent>(_ => { });

            parent.AddChild(child1);
            x.Attach(parent);

            x.Raise(new BasicEvent(), this);
            Assert.Equal(1, parent.Handled);
            Assert.Equal(1, child1.Handled);
            Assert.Equal(0, child2.Handled);

            parent.RemoveChild(child1);
            parent.AddChild(child2);
            x.Raise(new BasicEvent(), this);

            Assert.Equal(2, parent.Handled);
            Assert.Equal(1, child1.Handled);
            Assert.Equal(1, child2.Handled);

            parent.RemoveAll();
            x.Raise(new BasicEvent(), this);
            Assert.Equal(3, parent.Handled);
            Assert.Equal(1, child1.Handled);
            Assert.Equal(1, child2.Handled);

            parent.AddChild(child1);
            parent.AddChild(child2);
            child2.IsActive = false;
            x.Raise(new BasicEvent(), this);
            Assert.Equal(4, parent.Handled);
            Assert.Equal(2, child1.Handled);
            Assert.Equal(1, child2.Handled);

            parent.IsActive = false;
            x.Raise(new BasicEvent(), this);
            Assert.Equal(4, parent.Handled);
            Assert.Equal(2, child1.Handled);
            Assert.Equal(1, child2.Handled);

            parent.IsActive = true;
            x.Raise(new BasicEvent(), this);
            Assert.Equal(5, parent.Handled);
            Assert.Equal(3, child1.Handled);
            Assert.Equal(1, child2.Handled);

            parent.IsActive = true;
            child2.IsActive = true;
            x.Raise(new BasicEvent(), this);
            Assert.Equal(6, parent.Handled);
            Assert.Equal(4, child1.Handled);
            Assert.Equal(2, child2.Handled);
        }

        [Fact]
        public void DetachedChildTest()
        {
            var x = new EventExchange(new BasicLogExchange());
            var parent = new BasicComponent();
            var child = new BasicComponent();

            parent.AddHandler<BasicEvent>(_ => { });
            child.AddHandler<BasicEvent>(_ => { });

            parent.AddChild(child);
            x.Attach(parent);

            child.Remove();
            x.Raise(new BasicEvent(), this);
            Assert.Equal(1, parent.Handled);
            Assert.Equal(0, child.Handled);

            parent.IsActive = false;
            x.Raise(new BasicEvent(), this);
            Assert.Equal(1, parent.Handled);
            Assert.Equal(0, child.Handled);

            parent.IsActive = true;
            x.Raise(new BasicEvent(), this);
            Assert.Equal(2, parent.Handled);
            Assert.Equal(0, child.Handled);
        }

        [Fact]
        public void RemoveAllTest()
        {
            var x = new EventExchange(new BasicLogExchange());
            var parent = new BasicComponent();
            var child1 = new BasicComponent();
            var child2 = new BasicComponent();

            parent.AddHandler<BasicEvent>(_ => { });
            child1.AddHandler<BasicEvent>(_ => { });
            child2.AddHandler<BasicEvent>(_ => { });

            parent.AddChild(child1);
            x.Attach(parent);
            parent.AddChild(child2);

            x.Raise(new BasicEvent(), this);
            Assert.Equal(1, parent.Handled);
            Assert.Equal(1, child1.Handled);
            Assert.Equal(1, child2.Handled);

            parent.RemoveAll();

            x.Raise(new BasicEvent(), this);
            Assert.Equal(2, parent.Handled);
            Assert.Equal(1, child1.Handled);
            Assert.Equal(1, child2.Handled);

            var nonChild = new BasicComponent();
            parent.RemoveChild(nonChild);
        }

        [Fact]
        public void EnqueuedEventTest()
        {
            var x = new EventExchange(new BasicLogExchange());
            var c1 = new BasicComponent();
            var c2 = new BasicComponent();

            c1.AddHandler<BasicEvent>(_ => { });
            c2.AddHandler<BasicEvent>(_ => { });

            x.Attach(c1);
            x.Attach(c2);
            c1.Enqueue(new BasicEvent());
            Assert.Equal(0, c1.Handled);
            Assert.Equal(0, c2.Handled);
            x.FlushQueuedEvents();
            Assert.Equal(0, c1.Handled);
            Assert.Equal(1, c2.Handled);
        }
    }
}