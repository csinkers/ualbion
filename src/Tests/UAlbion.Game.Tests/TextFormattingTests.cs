using System;
using System.Linq;
using UAlbion.Config;
using UAlbion.Formats.Ids;
using UAlbion.Game.Text;
using Xunit;
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace UAlbion.Game.Tests;

public class TextFormattingTests
{
    static Action<(Token token, object arg)> Check(Token expectedToken, object expectedArg) => t =>
    {
        Assert.Equal(expectedToken, t.token);
        if (expectedArg == null)
            Assert.Null(t.arg);
        else
            Assert.Equal(expectedArg, t.arg);
    };

    [Fact]
    public void SimpleTokenTest()
    {
        var tokens = Tokeniser.Tokenise("").ToList();
        Assert.Empty(tokens);
        tokens = Tokeniser.Tokenise("test").ToList();
        Assert.Collection(tokens, (t) =>
        {
            Assert.Equal(Token.Text, t.Item1);
            Assert.Equal("test", t.Item2);
        });
    }

    [Fact]
    public void InventorySummaryTokenTest()
    {
        // test %%
        // test %s
        // test %d
        // {HE} tests {ME}
        // {HIS} test tested {HIM}
        var tokens = Tokeniser.Tokenise("{INVE}{NAME} ({SEXC}), %u years old, {RACE}, {CLAS}, level %d.").ToList();
        Assert.Collection(tokens, 
            Check(Token.Inventory, null),
            Check(Token.Name, null),
            Check(Token.Text, " ("),
            Check(Token.Sex, null),
            Check(Token.Text, "), "),
            Check(Token.Parameter, "u"),
            Check(Token.Text, " years old, "),
            Check(Token.Race, null),
            Check(Token.Text, ", "),
            Check(Token.Class, null),
            Check(Token.Text, ", level "),
            Check(Token.Parameter, "d"),
            Check(Token.Text, ".")
        );

    }

    [Fact]
    public void FullTokenTest()
    {
        var tokens = Tokeniser.Tokenise(
            "{HE}{HIM}{HIS}{ME}{CLAS}{RACE}{SEXC}{NAME}{DAMG}{PRIC}{COMB}" +
            "{INVE}{SUBJ}{VICT}{WEAP}{LEAD}{BIG }{FAT }{LEFT}{CNTR}{JUST}"+
            "{FAHI}{HIGH}{NORS}{TECF}{UNKN}{BLOK010}{INK 006}{WORDtoronto}"
        ).ToList();

        AssetMapping.GlobalIsThreadLocal = true;
        AssetMapping.Global.RegisterAssetType(typeof(Base.Ink), AssetType.Ink);

        Assert.Collection(tokens,
            Check(Token.He, null),
            Check(Token.Him, null),
            Check(Token.His, null),
            Check(Token.Me, null),
            Check(Token.Class, null),
            Check(Token.Race, null),
            Check(Token.Sex, null),
            Check(Token.Name, null),
            Check(Token.Damage, null),
            Check(Token.Price, null),
            Check(Token.Combatant, null),
            Check(Token.Inventory, null),
            Check(Token.Subject, null),
            Check(Token.Victim, null),
            Check(Token.Weapon, null),
            Check(Token.Leader, null),
            Check(Token.Big , null),
            Check(Token.Fat , null),
            Check(Token.Left, null),
            Check(Token.Centre, null),
            Check(Token.Justify, null),
            Check(Token.FatHigh, null),
            Check(Token.High, null),
            Check(Token.NormalSize, null),
            Check(Token.Tecf, null),
            Check(Token.Unknown, null),
            Check(Token.Block, 10),
            Check(Token.Ink, (InkId)(Base.Ink)6),
            Check(Token.Word, "toronto")
        );
    }

    [Fact]
    public void MultilineTokenTest()
    {
        AssetMapping.GlobalIsThreadLocal = true;
        AssetMapping.Global.RegisterAssetType(typeof(Base.Ink), AssetType.Ink);

        var tokens = Tokeniser.Tokenise("{INK 006}{CNTR}{BIG }Title.^^{NORS}Description").ToList();
        Assert.Collection(tokens,
            Check(Token.Ink, (InkId)(Base.Ink)6),
            Check(Token.Centre, null),
            Check(Token.Big, null),
            Check(Token.Text, "Title."),
            Check(Token.NewLine, null),
            Check(Token.NewLine, null),
            Check(Token.NormalSize, null),
            Check(Token.Text, "Description")
        );
    }

    [Fact]
    public void PercentageTokenTest()
    {
        var tokens = Tokeniser.Tokenise("Test 100%%").ToList();
        Assert.Collection(tokens,
            Check(Token.Text, "Test 100"),
            Check(Token.Text, "%")
        );
    }

    [Fact]
    public void ParameterTokenTest()
    {
        var tokens = Tokeniser.Tokenise("Test %ld %s: %d (%u) Other%z.").ToList();
        Assert.Collection(tokens,
            Check(Token.Text, "Test "),
            Check(Token.Parameter, "ld"),
            Check(Token.Text, " "),
            Check(Token.Parameter, "s"),
            Check(Token.Text, ": "),
            Check(Token.Parameter, "d"),
            Check(Token.Text, " ("),
            Check(Token.Parameter, "u"),
            Check(Token.Text, ") Other"),
            Check(Token.Text, "%z"),
            Check(Token.Text, ".")
        );
    }
}