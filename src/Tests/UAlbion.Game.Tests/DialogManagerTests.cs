using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Eventing;
using Xunit;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Game.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.Gui;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;
using UAlbion.TestCommon;

namespace UAlbion.Game.Tests;

public class DialogManagerTests
{
    [Fact]
    public void YesNoPromptTest()
    {
        AssetMapping.GlobalIsThreadLocal = true;
        AssetMapping.Global.Clear()
            .RegisterAssetType(typeof(Base.SystemText), AssetType.Text)
            .RegisterAssetType(typeof(Base.Font), AssetType.Font)
            ;

        var systemText = new Dictionary<TextId, string>
        {
            { Base.SystemText.MainMenu_DoYouReallyWantToQuit, "Do you really want to quit?"},
            { Base.SystemText.MsgBox_Yes, "Yes"},
            { Base.SystemText.MsgBox_No, "No"}
        };

        var ex = new EventExchange(new LogExchange());
        var dm = new DialogManager();
        var lm = new LayoutManager();
        var mma = new MockModApplier()
                .Add(new AssetId(AssetType.MetaFont, (ushort)new MetaFontId(false, FontColor.White)), MockUniformFont.Font(AssetId.From(Base.Font.RegularFont)))
                .AddInfo(AssetId.From(Base.Font.RegularFont), MockUniformFont.Info)
            ;

        foreach (var kvp in systemText)
            mma.Add(kvp.Key, kvp.Value);

        ex
            .Attach(mma)
            .Attach(new AssetManager())
            .Attach(new SpriteManager<SpriteInfo>())
            .Attach(new MockGameFactory())
            .Attach(new WordLookup())
            .Attach(new TextFormatter())
            .Attach(new TextManager())
            .Attach(new WindowManager { Resolution = (1920, 1080) })
            .Attach(new MockSettings { Language = Base.Language.English })
            .Attach(dm)
            .Attach(lm)
            ;

        var e = new YesNoPromptEvent((TextId)Base.SystemText.MainMenu_DoYouReallyWantToQuit);

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
        e = new YesNoPromptEvent((TextId)Base.SystemText.MainMenu_DoYouReallyWantToQuit);
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