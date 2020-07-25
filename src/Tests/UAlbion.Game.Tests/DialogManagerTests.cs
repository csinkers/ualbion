using System.Collections.Generic;
using System.Linq;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.Gui;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;
using UAlbion.TestCommon;
using Xunit;

namespace UAlbion.Game.Tests
{
    public class DialogManagerTests
    {
        [Fact]
        public void YesNoPromptTest()
        {
            var systemText = new Dictionary<int, string>
            {
                { (int)SystemTextId.MainMenu_DoYouReallyWantToQuit, "Do you really want to quit?"},
                { (int)SystemTextId.MsgBox_Yes, "Yes"},
                { (int)SystemTextId.MsgBox_No, "No"}
            };

            var ex = new EventExchange(new LogExchange());
            var dm = new DialogManager();
            var lm = new LayoutManager();
            var alr = new MockAssetLocatorRegistry()
                .Add(new AssetKey(AssetType.SystemText), systemText)
                .Add(new AssetKey(AssetType.MetaFont, (ushort)new MetaFontId()), MockUniformFont.Font)
                ;

            ex
                .Attach(alr)
                .Attach(new AssetManager())
                .Attach(new TextFormatter())
                .Attach(new TextManager())
                .Attach(new SpriteManager())
                .Attach(new WindowManager { Window = new MockWindow(1920, 1080) })
                .Attach(new MockSettings { Language = GameLanguage.English })
                .Attach(dm)
                .Attach(lm)
                ;

            var e = new YesNoPromptEvent(SystemTextId.MainMenu_DoYouReallyWantToQuit);

            bool? result = null;
            ex.RaiseAsync<bool>(e, null, x => result = x);
            Assert.Null(result);

            var layout = lm.GetLayout();
            Assert.Equal(1, layout.Children.Count); // Should only be one top-level dialog
            var yesText = layout.DepthFirstSearch(x => x.Element is TextLine txt && txt.ToString().Contains("\"Yes\"")).First();
            var yesButton = (Button)yesText.Ancestors.First(x => x.Element is Button).Element;
            yesButton.Receive(new HoverEvent(), null);
            yesButton.Receive(new UiLeftClickEvent(), null);
            yesButton.Receive(new UiLeftReleaseEvent(), null);

            Assert.True(result);
            layout = lm.GetLayout();
            Assert.Equal(0, layout.Children.Count); // Dialog should be closed, so no top-level dialogs

            // Open another yes/no dialog
            e = new YesNoPromptEvent(SystemTextId.MainMenu_DoYouReallyWantToQuit);
            ex.RaiseAsync<bool>(e, this, x => result = x);
            layout = lm.GetLayout();
            Assert.Equal(1, layout.Children.Count); // Should only be one top-level dialog
            var noText = layout.DepthFirstSearch(x => x.Element is TextLine txt && txt.ToString().Contains("\"No\"")).First();
            var noButton = (Button)noText.Ancestors.First(x => x.Element is Button).Element;
            noButton.Receive(new HoverEvent(), null);
            noButton.Receive(new UiLeftClickEvent(), null);
            noButton.Receive(new UiLeftReleaseEvent(), null);
            Assert.False(result);
        }
    }
}
