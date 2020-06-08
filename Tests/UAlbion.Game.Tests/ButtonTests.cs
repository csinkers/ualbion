using System.Linq;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.Gui;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;
using Xunit;

namespace UAlbion.Game.Tests
{
    public class ButtonTests : Component
    {
        readonly EventExchange _exchange;

        class PublicFrameButton : Button
        {
            public PublicFrameButton(IUiElement content) : base(content) { } 
            public PublicFrameButton(IText textSource) : base(textSource) { } 
            public PublicFrameButton(StringId textId) : base(textId) { } 
            public PublicFrameButton(string literalText) : base(literalText) { }
            public ButtonFrame Frame => Children.OfType<ButtonFrame>().First();
        }

        public ButtonTests()
        {
            var fakeFont = new MockTexture(
                "FakeFont", 6, 8,
                new byte[6*8*256],
                Enumerable.Range(0, 256).Select(x => 
                    new SubImage(
                        new Vector2(x * 6, 0),
                        new Vector2(6, 8),
                        new Vector2(6 * 256, 8),
                        0)) );

            _exchange = new EventExchange(new LogExchange());
            var factory = new MockFactory();
            var locator = new MockAssetLocatorRegistry()
                .Add(new AssetKey(AssetType.MetaFont, (ushort)new MetaFontId()), fakeFont)
                ;

            _exchange
                .Attach(locator)
                .Attach(new AssetManager())
                .Attach(new SpriteManager())
                .Attach(new WindowManager { Window = new MockWindow(1920, 1080) })
                .Attach(new TextManager())
                .Register<ICommonColors>(new CommonColors(factory))
                .Attach(this)
                ;
        }

        [Fact]
        public void RenderTest()
        {
            var button = new Button("Test");
            _exchange.Attach(button);
            var size = button.GetSize();
            var order = button.Render(new Rectangle(0, 0, (int)size.X, (int)size.Y), 0);
            var size2 = button.GetSize();
            Assert.Equal(size, size2);
            Assert.Equal(4, order);
        }

        [Fact]
        public void ClickTest()
        {
            int downCount = 0;
            int clickCount = 0;
            var button = (PublicFrameButton)new PublicFrameButton("Test")
                .OnButtonDown(() => downCount++)
                .OnClick(() => clickCount++);

            _exchange.Attach(button);
            Assert.Equal(ButtonState.Normal, button.Frame.State);

            button.Receive(new HoverEvent(), null);
            Assert.Equal(ButtonState.Hover, button.Frame.State);

            button.Receive(new UiLeftClickEvent(), null);
            Assert.Equal(1, downCount);
            Assert.Equal(0, clickCount);
            Assert.Equal(ButtonState.Clicked, button.Frame.State);

            button.Receive(new UiLeftReleaseEvent(), null);
            Assert.Equal(1, downCount);
            Assert.Equal(1, clickCount);
            Assert.Equal(ButtonState.Hover, button.Frame.State);

            button.Receive(new UiLeftClickEvent(), null);
            Assert.Equal(2, downCount);
            Assert.Equal(1, clickCount);
            Assert.Equal(ButtonState.Clicked, button.Frame.State);

            button.Receive(new BlurEvent(), null);
            Assert.Equal(ButtonState.ClickedBlurred, button.Frame.State);

            button.Receive(new UiLeftReleaseEvent(), null);
            Assert.Equal(2, downCount);
            Assert.Equal(1, clickCount);
            Assert.Equal(ButtonState.Normal, button.Frame.State);
        }

        [Fact]
        public void ClickDoubleClickableTest()
        {
            int downCount = 0;
            int clickCount = 0;
            int doubleClickCount = 0;
            var button = (PublicFrameButton)new PublicFrameButton("Test")
                .OnButtonDown(() => downCount++)
                .OnClick(() => clickCount++)
                .OnDoubleClick(() => doubleClickCount++)
                ;

            string timerId = null;
            On<StartTimerEvent>(e => timerId = e.Id);

            _exchange.Attach(button);
            Assert.Equal(ButtonState.Normal, button.Frame.State);

            button.Receive(new HoverEvent(), null);
            Assert.Equal(ButtonState.Hover, button.Frame.State);

            button.Receive(new UiLeftClickEvent(), null);
            Assert.Equal(1, downCount);
            Assert.Equal(0, clickCount);
            Assert.Equal(ButtonState.Clicked, button.Frame.State);

            button.Receive(new UiLeftReleaseEvent(), null);
            Assert.NotNull(timerId);
            Assert.Equal(1, downCount);
            Assert.Equal(0, clickCount);
            Assert.Equal(ButtonState.Hover, button.Frame.State);

            Raise(new TimerElapsedEvent(timerId));
            Assert.Equal(1, clickCount);
            timerId = null;

            button.Receive(new UiLeftClickEvent(), null);
            Assert.Equal(2, downCount);
            Assert.Equal(1, clickCount);
            Assert.Equal(ButtonState.Clicked, button.Frame.State);

            button.Receive(new BlurEvent(), null);
            Assert.Equal(ButtonState.ClickedBlurred, button.Frame.State);

            button.Receive(new UiLeftReleaseEvent(), null);
            Assert.Equal(2, downCount);
            Assert.Equal(1, clickCount);
            Assert.Equal(ButtonState.Normal, button.Frame.State);
            Assert.Null(timerId);
        }

        [Fact]
        public void RightClickTest()
        {
            int clickCount = 0;
            var button = (PublicFrameButton)new PublicFrameButton("Test")
                .OnRightClick(() => clickCount++);

            _exchange.Attach(button);
            Assert.Equal(ButtonState.Normal, button.Frame.State);

            button.Receive(new HoverEvent(), null);
            Assert.Equal(ButtonState.Hover, button.Frame.State);

            button.Receive(new UiRightClickEvent(), null);
            Assert.Equal(0, clickCount);
            Assert.Equal(ButtonState.Hover, button.Frame.State);

            button.Receive(new UiRightReleaseEvent(), null);
            Assert.Equal(1, clickCount);
            Assert.Equal(ButtonState.Hover, button.Frame.State);

            button.Receive(new UiRightClickEvent(), null);
            Assert.Equal(1, clickCount);
            Assert.Equal(ButtonState.Hover, button.Frame.State);

            button.Receive(new BlurEvent(), null);
            Assert.Equal(ButtonState.Normal, button.Frame.State);

            button.Receive(new UiRightReleaseEvent(), null);
            Assert.Equal(1, clickCount);
            Assert.Equal(ButtonState.Normal, button.Frame.State);
        }

        [Fact]
        public void DoubleClickTest()
        {
            int downCount = 0;
            int clickCount = 0;
            int doubleClickCount = 0;
            var button = (PublicFrameButton)new PublicFrameButton("Test")
                .OnButtonDown(() => downCount++)
                .OnClick(() => clickCount++)
                .OnDoubleClick(() => doubleClickCount++)
                ;

            string timerId = null;
            On<StartTimerEvent>(e => timerId = e.Id);

            _exchange.Attach(button);
            Assert.Equal(ButtonState.Normal, button.Frame.State);

            button.Receive(new HoverEvent(), null);
            Assert.Equal(ButtonState.Hover, button.Frame.State);

            button.Receive(new UiLeftClickEvent(), null);
            Assert.Equal(1, downCount);
            Assert.Equal(0, clickCount);
            Assert.Equal(0, doubleClickCount);
            Assert.Equal(ButtonState.Clicked, button.Frame.State);

            button.Receive(new UiLeftReleaseEvent(), null);
            Assert.NotNull(timerId);
            Assert.Equal(1, downCount);
            Assert.Equal(0, clickCount);
            Assert.Equal(0, doubleClickCount);
            Assert.Equal(ButtonState.Hover, button.Frame.State);

            button.Receive(new UiLeftClickEvent(), null);
            Assert.Equal(2, downCount);
            Assert.Equal(0, clickCount);
            Assert.Equal(0, doubleClickCount);
            Assert.Equal(ButtonState.Clicked, button.Frame.State);

            button.Receive(new UiLeftReleaseEvent(), null);
            Assert.Equal(2, downCount);
            Assert.Equal(0, clickCount);
            Assert.Equal(1, doubleClickCount);
            Assert.Equal(ButtonState.Hover, button.Frame.State);

            Raise(new TimerElapsedEvent(timerId));
            Assert.Equal(2, downCount);
            Assert.Equal(0, clickCount);
            Assert.Equal(1, doubleClickCount);
            timerId = null;
        }

        [Fact]
        public void HoverTest()
        {
            int hoverCount = 0;
            int blurCount = 0;

            var button = (PublicFrameButton)new PublicFrameButton("Test")
                .OnHover(() => hoverCount++)
                .OnBlur(() => blurCount++);

            _exchange.Attach(button);
            Assert.Equal(ButtonState.Normal, button.Frame.State);

            button.Receive(new HoverEvent(), null);
            Assert.Equal(ButtonState.Hover, button.Frame.State);
            Assert.Equal(1, hoverCount);
            Assert.Equal(0, blurCount);

            button.Receive(new BlurEvent(), null);
            Assert.Equal(ButtonState.Normal, button.Frame.State);
            Assert.Equal(1, hoverCount);
            Assert.Equal(1, blurCount);
        }

        [Fact] public void PressedTest() {}

        [Fact]
        public void TypematicTest()
        {
            int downCount = 0;
            int clickCount = 0;
            var button = (PublicFrameButton)new PublicFrameButton("Test") { Typematic = true }
                .OnButtonDown(() => downCount++)
                .OnClick(() => clickCount++);

            _exchange.Attach(button);
            Assert.Equal(ButtonState.Normal, button.Frame.State);

            button.Receive(new UiLeftClickEvent(), null);
            Assert.Equal(0, downCount);
            Assert.Equal(0, clickCount);
            Assert.Equal(ButtonState.Normal, button.Frame.State);

            button.Receive(new HoverEvent(), null);
            Assert.Equal(ButtonState.Hover, button.Frame.State);

            button.Receive(new UiLeftClickEvent(), null);
            Assert.Equal(1, downCount);
            Assert.Equal(1, clickCount);
            Assert.Equal(ButtonState.Clicked, button.Frame.State);

            button.Receive(new UiLeftReleaseEvent(), null);
            Assert.Equal(1, downCount);
            Assert.Equal(1, clickCount);
            Assert.Equal(ButtonState.Hover, button.Frame.State);

            button.Receive(new UiLeftClickEvent(), null);
            Assert.Equal(2, downCount);
            Assert.Equal(2, clickCount);
            Assert.Equal(ButtonState.Clicked, button.Frame.State);

            button.Receive(new BlurEvent(), null);
            Assert.Equal(ButtonState.ClickedBlurred, button.Frame.State);

            button.Receive(new UiLeftReleaseEvent(), null);
            Assert.Equal(2, downCount);
            Assert.Equal(2, clickCount);
            Assert.Equal(ButtonState.Normal, button.Frame.State);

            button.Receive(new HoverEvent(), null);
            button.Receive(new UiLeftClickEvent(), null);
            Assert.Equal(3, downCount);
            Assert.Equal(3, clickCount);

        }
    }
}
