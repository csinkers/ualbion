using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using Xunit;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
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
            .RegisterAssetType(typeof(Base.Palette), AssetType.Palette)
            .RegisterAssetType(typeof(Base.CoreGfx), AssetType.CoreGfx)
            .RegisterAssetType(typeof(Base.Ink), AssetType.Ink)
            .RegisterAssetType(typeof(Base.Font), AssetType.FontDefinition)
            .RegisterAssetType(typeof(Base.FontGfx), AssetType.FontGfx)
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
        var comPalId = (PaletteId)Base.Palette.Common;
        var palId = (PaletteId)Base.Palette.Inventory;

        var font = MockUniformFont.BuildFontDefinition();
        var mma = new MockModApplier()
                .Add((FontId)Base.Font.Regular, font)
                .Add((InkId)Base.Ink.White, MockUniformFont.BuildInk())
                .Add((SpriteId)Base.FontGfx.RegularFont, MockUniformFont.BuildFontTexture())
                .Add(comPalId, new AlbionPalette(comPalId.ToUInt32(), "PCommon", Enumerable.Repeat(0xffffffu, 256).ToArray()))
                .Add(palId, new AlbionPalette(palId.ToUInt32(), "PInv", Enumerable.Repeat(0xffffffu, 256).ToArray()))
            ;

        void AddDummySprite(SpriteId id)
        {
            mma.Add(id, new SimpleTexture<byte>(
                id,
                id.ToString(),
                2, 2,
                new byte[] { 0, 1, 2, 3 },
                new[] { new Region(0, 0, 2, 2, 2, 2, 0) }));
        }

        AddDummySprite(Base.CoreGfx.UiBackground);
        AddDummySprite(Base.CoreGfx.UiBackgroundLines1);
        AddDummySprite(Base.CoreGfx.UiBackgroundLines2);
        AddDummySprite(Base.CoreGfx.UiBackgroundLines3);
        AddDummySprite(Base.CoreGfx.UiBackgroundLines4);
        AddDummySprite(Base.CoreGfx.UiWindowTopLeft);
        AddDummySprite(Base.CoreGfx.UiWindowTopRight);
        AddDummySprite(Base.CoreGfx.UiWindowBottomLeft);
        AddDummySprite(Base.CoreGfx.UiWindowBottomRight);

        foreach (var kvp in systemText)
            mma.Add(kvp.Key, kvp.Value);

        var assetManager = new AssetManager();
        ex
            .Attach(mma)
            .Attach(assetManager)
            .Attach(new MockSettings())
            .Attach(new SpriteManager<SpriteInfo>())
            .Attach(new MockGameFactory())
            .Attach(new WordLookup())
            .Attach(new TextFormatter())
            .Attach(new TextManager())
            .Attach(new WindowManager { Resolution = (1920, 1080) })
            .Attach(dm)
            .Attach(lm)
            .Register<ICommonColors>(new CommonColors())
            ;

        var metaFont = font.Build(Base.Font.Regular, Base.Ink.White, assetManager);
        mma.Add(metaFont.Id, metaFont);

        var e = new YesNoPromptEvent((TextId)Base.SystemText.MainMenu_DoYouReallyWantToQuit);

        bool? result = null;
        ex.RaiseAsync(e, null, x => result = x);
        Assert.Null(result);

        var layout = lm.GetLayout();
        Assert.Equal(1, layout.Children.Count); // Should only be one top-level dialog
        var yesText = layout.DepthFirstSearch(x => x.Element is UiTextLine txt && txt.ToString().Contains("\"Yes\"")).First();
        var yesButton = (Button)yesText.Ancestors.First(x => x.Element is Button).Element;
        yesButton.Receive(new HoverEvent(), null);
        yesButton.Receive(new UiLeftClickEvent(), null);
        yesButton.Receive(new UiLeftReleaseEvent(), null);

        Assert.True(result);
        layout = lm.GetLayout();
        Assert.Equal(0, layout.Children.Count); // Dialog should be closed, so no top-level dialogs

        // Open another yes/no dialog
        e = new YesNoPromptEvent((TextId)Base.SystemText.MainMenu_DoYouReallyWantToQuit);
        ex.RaiseAsync(e, this, x => result = x);
        layout = lm.GetLayout();
        Assert.Equal(1, layout.Children.Count); // Should only be one top-level dialog
        var noText = layout.DepthFirstSearch(x => x.Element is UiTextLine txt && txt.ToString().Contains("\"No\"")).First();
        var noButton = (Button)noText.Ancestors.First(x => x.Element is Button).Element;
        noButton.Receive(new HoverEvent(), null);
        noButton.Receive(new UiLeftClickEvent(), null);
        noButton.Receive(new UiLeftReleaseEvent(), null);
        Assert.False(result);
    }
}