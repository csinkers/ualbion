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
            var c2 = new BasicComponent();
            var e = new BasicEvent();
            var ee = new EventExchange(new BasicLogExchange());

            ee.Attach(c1);
            ee.Attach(c2);

            Assert.Equal(0, c1.Handled);
            Assert.Equal(0, c2.Handled);

            c1.CallRaise(e);

            Assert.Equal(0, c1.Handled); // Components shouldn't see their own events.
            Assert.Equal(1, c2.Handled);

            var recipients = ee.EnumerateRecipients(typeof(BasicEvent)).ToList();
            Assert.Collection(recipients,
                x => Assert.Equal(x, c1),
                x => Assert.Equal(x, c2));
        }

        [Fact]
        public void DisableHandlerTest()
        {
            var c = new BasicComponent();
            var e = new BasicEvent();
            var x = new EventExchange(new BasicLogExchange());

            c.Attach(x);
            Assert.Equal(0, c.Handled);

            c.Receive(e, this);
            Assert.Equal(1, c.Handled);

            c.EnableHandler();
            c.Receive(e, this);
            Assert.Equal(2, c.Handled);

            c.DisableHandler();
            c.Receive(e, this);
            Assert.Equal(2, c.Handled);

            c.EnableHandler();
            c.Receive(e, this);
            Assert.Equal(3, c.Handled);
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

            parent.Add(child1);
            x.Attach(parent);

            x.Raise(new BasicEvent(), this);
            Assert.Equal(1, parent.Handled);
            Assert.Equal(1, child1.Handled);
            Assert.Equal(0, child2.Handled);

            parent.Remove(child1);
            parent.Add(child2);
            x.Raise(new BasicEvent(), this);

            Assert.Equal(2, parent.Handled);
            Assert.Equal(1, child1.Handled);
            Assert.Equal(1, child2.Handled);

            parent.RemoveAll();
            x.Raise(new BasicEvent(), this);
            Assert.Equal(3, parent.Handled);
            Assert.Equal(1, child1.Handled);
            Assert.Equal(1, child2.Handled);

            parent.Add(child1);
            parent.Add(child2);
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

            parent.Add(child);
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

            parent.Add(child1);
            x.Attach(parent);
            parent.Add(child2);

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
            parent.Remove(nonChild);
        }

        [Fact]
        public void EnqueuedEventTest()
        {
            var x = new EventExchange(new BasicLogExchange());
            var c1 = new BasicComponent();
            var c2 = new BasicComponent();
            x.Attach(c1);
            x.Attach(c2);
            c1.CallEnqueue(new BasicEvent());
            Assert.Equal(0, c1.Handled);
            Assert.Equal(0, c2.Handled);
            x.FlushQueuedEvents();
            Assert.Equal(0, c1.Handled);
            Assert.Equal(1, c2.Handled);
        }
    }
}