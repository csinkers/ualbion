using System.Collections.Generic;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting
{
    public static class ScriptParser
    {
        public static readonly TokenListParser<ScriptToken, ICfgNode> Name =
            Token.EqualTo(ScriptToken.Identifier)
                .Select(x => (ICfgNode)Emit.Name(x.ToStringValue())).Named("Name");

        public static readonly TokenListParser<ScriptToken, ICfgNode> Number =
            Token.EqualTo(ScriptToken.Number)
                .Apply(Numerics.IntegerInt32)
                .Select(x => (ICfgNode)Emit.Const(x)).Named("Number");

        public static readonly TokenListParser<ScriptToken, ICfgNode> Identifier = Name.Or(Number).Named("Identifier");

        public static readonly TokenListParser<ScriptToken, ICfgNode> Factor =
            Parse.Ref(() => Expression).Between(
                    Token.EqualTo(ScriptToken.LParen),
                    Token.EqualTo(ScriptToken.RParen))
                .Or(Identifier);


        public static readonly TokenListParser<ScriptToken, ICfgNode> Negation =
            (from op in Token.EqualTo(ScriptToken.Not)
            from val in Factor
            select (ICfgNode)Emit.Negation(val)).Named("Negation");

        public static readonly TokenListParser<ScriptToken, ScriptOp> Member = Token.EqualTo(ScriptToken.Dot).Value(ScriptOp.Member);
        public static readonly TokenListParser<ScriptToken, ScriptOp> Eq  = Token.EqualTo(ScriptToken.Equal).Value(ScriptOp.Equal);
        public static readonly TokenListParser<ScriptToken, ScriptOp> Neq = Token.EqualTo(ScriptToken.NotEqual).Value(ScriptOp.NotEqual);
        public static readonly TokenListParser<ScriptToken, ScriptOp> Gt  = Token.EqualTo(ScriptToken.Greater).Value(ScriptOp.Greater);
        public static readonly TokenListParser<ScriptToken, ScriptOp> Gte = Token.EqualTo(ScriptToken.GreaterEqual).Value(ScriptOp.GreaterEqual);
        public static readonly TokenListParser<ScriptToken, ScriptOp> Lte = Token.EqualTo(ScriptToken.LesserEqual).Value(ScriptOp.LesserEqual);
        public static readonly TokenListParser<ScriptToken, ScriptOp> Lt  = Token.EqualTo(ScriptToken.Lesser).Value(ScriptOp.Lesser);
        public static readonly TokenListParser<ScriptToken, ScriptOp> Assign = Token.EqualTo(ScriptToken.Assign).Value(ScriptOp.Assign);
        public static readonly TokenListParser<ScriptToken, ScriptOp> Add = Token.EqualTo(ScriptToken.Add).Value(ScriptOp.Add);
        public static readonly TokenListParser<ScriptToken, ScriptOp> Sub = Token.EqualTo(ScriptToken.Sub).Value(ScriptOp.Subtract);
        public static readonly TokenListParser<ScriptToken, ScriptOp> And = Token.EqualTo(ScriptToken.And).Value(ScriptOp.And);
        public static readonly TokenListParser<ScriptToken, ScriptOp> Or = Token.EqualTo(ScriptToken.Or).Value(ScriptOp.Or);

        /*
        1 LtR () [] . (postfix inc/dec)
        2 RtL ! (prefix inc/dec)       
        3 LtR < <= > >=                
        4 LtR == !=                    
        5 LtR &&                       
        6 LtR ||                       
        7 RtL = += -=                  
        8 LtR , 
        */

        public static readonly TokenListParser<ScriptToken, ICfgNode> Op1 = Parse.Chain(Member, Factor, Emit.Op);
        public static readonly TokenListParser<ScriptToken, ICfgNode> Op2 = Negation.Or(Op1);
        public static readonly TokenListParser<ScriptToken, ICfgNode> Op3 = Parse.Chain(Lte.Or(Lt).Or(Gte).Or(Gt), Op2, Emit.Op);
        public static readonly TokenListParser<ScriptToken, ICfgNode> Op4 = Parse.Chain(Eq.Or(Neq), Op3, Emit.Op);
        public static readonly TokenListParser<ScriptToken, ICfgNode> Op5 = Parse.Chain(And, Op4, Emit.Op);
        public static readonly TokenListParser<ScriptToken, ICfgNode> Op6 = Parse.Chain(Or, Op5, Emit.Op);
        public static readonly TokenListParser<ScriptToken, ICfgNode> Op7 = Parse.ChainRight(Assign.Or(Add).Or(Sub), Op6, Emit.Op);

        public static readonly TokenListParser<ScriptToken, ICfgNode> Expression =
            (from first in Op7
             from rest in Op7.Many()
             select rest.Length == 0 ? first : Emit.Statement(first, rest)).Named("Expression");

        public static readonly TokenListParser<ScriptToken, ICfgNode> EventStatement =
            (from first in Op7
             from rest in Op7.Many()
             select (ICfgNode)Emit.Statement(first, rest)).Named("EventStatement");

        public static readonly TokenListParser<ScriptToken, ICfgNode> Label =
            (from name in Token.EqualTo(ScriptToken.Identifier)
            from colon in Token.EqualTo(ScriptToken.Colon)
            select (ICfgNode)Emit.Label(name.ToStringValue())).Named("Label");

        public static readonly TokenListParser<ScriptToken, ICfgNode> If =
            (from keyword in Token.EqualToValue(ScriptToken.Identifier, "if")
            from lp in Token.EqualTo(ScriptToken.LParen)
            from _ in Token.EqualTo(ScriptToken.NewLine).Many()
            from condition in Expression
            from rp in Token.EqualTo(ScriptToken.RParen)
            from body in Parse.Ref(() => Statement)
            select (ICfgNode)Emit.If(condition, body)).Named("If");

        public static readonly TokenListParser<ScriptToken, ICfgNode> IfElse =
            (from keyword in Token.EqualToValue(ScriptToken.Identifier, "if")
            from lp in Token.EqualTo(ScriptToken.LParen)
            from _ in Token.EqualTo(ScriptToken.NewLine).Many()
            from condition in Expression
            from rp in Token.EqualTo(ScriptToken.RParen)
            from body in Parse.Ref(() => Statement)
            from keyword2 in Token.EqualToValue(ScriptToken.Identifier, "else")
            from elseBody in Parse.Ref(() => Statement)
            select (ICfgNode)Emit.IfElse(condition, body, elseBody)).Named("IfElse");

        public static readonly TokenListParser<ScriptToken, ICfgNode> While =
            (from keyword in Token.EqualToValue(ScriptToken.Identifier, "while")
            from lp in Token.EqualTo(ScriptToken.LParen)
            from _ in Token.EqualTo(ScriptToken.NewLine).Many()
            from condition in Expression
            from rp in Token.EqualTo(ScriptToken.RParen)
            from body in Parse.Ref(() => Statement)
            select (ICfgNode)Emit.While(condition, body)).Named("While");

        public static readonly TokenListParser<ScriptToken, ICfgNode> Do =
            (from keyword in Token.EqualToValue(ScriptToken.Identifier, "do")
            from body in Parse.Ref(() => Statement)
            from keyword2 in Token.EqualToValue(ScriptToken.Identifier, "while")
            from lp in Token.EqualTo(ScriptToken.LParen)
            from _ in Token.EqualTo(ScriptToken.NewLine).Many()
            from condition in Expression
            from rp in Token.EqualTo(ScriptToken.RParen)
            select (ICfgNode)Emit.Do(condition, body)).Named("Do");

        public static readonly TokenListParser<ScriptToken, ICfgNode> Loop =
            (from keyword in Token.EqualToValue(ScriptToken.Identifier, "loop")
            from body in Parse.Ref(() => Statement)
            select (ICfgNode)Emit.Loop(body)).Named("EndlessLoop");

        public static readonly TokenListParser<ScriptToken, ICfgNode> Break =
            Token.EqualToValue(ScriptToken.Identifier, "break").Value((ICfgNode)Emit.Break());
        public static readonly TokenListParser<ScriptToken, ICfgNode> Continue =
            Token.EqualToValue(ScriptToken.Identifier, "continue").Value((ICfgNode)Emit.Continue());

        public static readonly TokenListParser<ScriptToken, ICfgNode> Goto =
            from keyword in Token.EqualToValue(ScriptToken.Identifier, "goto")
            from label in Token.EqualTo(ScriptToken.Identifier)
            select (ICfgNode)Emit.Goto(label.ToStringValue());

        public static readonly TokenListParser<ScriptToken, ICfgNode> SingleStatement =
            Label.Try()
            .Or(IfElse).Try()
            .Or(If)
            .Or(While)
            .Or(Do)
            .Or(Loop)
            .Or(Break)
            .Or(Continue)
            .Or(Goto)
            .Or(EventStatement);

        static ICfgNode MakeSeq(ICfgNode[] statements) =>
            statements.Length switch
            {
                0 => Emit.Empty(),
                1 => statements[0],
                _ => Emit.Seq(statements)
            };

        public static readonly TokenListParser<ScriptToken, Token<ScriptToken>> Comma = Token.EqualTo(ScriptToken.Comma);
        public static readonly TokenListParser<ScriptToken, Token<ScriptToken>> NewLine = Token.EqualTo(ScriptToken.NewLine);

        public static readonly TokenListParser<ScriptToken, ICfgNode> Sequence =
            (from statements in SingleStatement.ManyDelimitedBy(Comma.Or(NewLine))
             select MakeSeq(statements)).Named("Sequence");

        public static readonly TokenListParser<ScriptToken, ICfgNode> Block =
            (from lb in Token.EqualTo(ScriptToken.LBrace)
             from _ in NewLine.Many()
            from body in Sequence
            from rb in Token.EqualTo(ScriptToken.RBrace)
            select body).Named("Block");

        public static readonly TokenListParser<ScriptToken, ICfgNode> Statement =
            SingleStatement
            .Or(Block)
            .Named("Statement");

        public static readonly TokenListParser<ScriptToken, ICfgNode> TopLevel = Sequence.AtEnd();
        public static readonly TokenListParser<ScriptToken, ICfgNode[]> Blocks = 
            (from ws in NewLine.Many()
            from block in Block
            from ws2 in NewLine.Many()
            select block).Many();

        public static TokenList<ScriptToken> FilterTokens(TokenList<ScriptToken> tokens)
        {
            var filtered = new List<Token<ScriptToken>>();
            Token<ScriptToken> last = Token<ScriptToken>.Empty;
            foreach (var token in tokens)
            {
                if (last.HasValue)
                {
                    var skip = (last.Kind, token.Kind) switch
                    {
                        (ScriptToken.NewLine, ScriptToken.NewLine) => true,
                        (ScriptToken.NewLine, ScriptToken.RBrace) => true,
                        _ => false
                    };

                    if (!skip)
                        filtered.Add(last);
                }

                last = token;
            }

            if (last.HasValue)
                filtered.Add(last);

            return new TokenList<ScriptToken>(filtered.ToArray());
        }

        public static bool TryParse(string source, out ICfgNode[] abstractSyntaxTrees, out string error, out Position errorPosition)
        {
            var tokens = ScriptTokenizer.Tokenize(source);
            if (!tokens.HasValue)
            {
                abstractSyntaxTrees = null;
                error = "Tokenisation error, " + tokens;
                errorPosition = tokens.ErrorPosition;
                return false;
            }

            var filtered = FilterTokens(tokens.Value);
            var parsed = Blocks.TryParse(filtered);

            if (!parsed.HasValue)
            {
                abstractSyntaxTrees = null;
                error = parsed.ToString();
                errorPosition = parsed.ErrorPosition;
                return false;
            }

            abstractSyntaxTrees = parsed.Value;
            error = null;
            errorPosition = Position.Empty;
            return true;
        }
    }
}
