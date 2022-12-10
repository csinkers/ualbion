using System;
using System.Globalization;
using UAlbion.Api.Eventing;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting;

public class FormatScriptVisitor : IAstVisitor
{
    readonly IScriptBuilder _builder;
    readonly long _initialPos;
    bool _inCondition;

    public FormatScriptVisitor(IScriptBuilder builder)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        _initialPos = _builder.Length;
    }

    public bool PrettyPrint { get; init; } = true;
    public bool WrapStatements { get; init; } = true;
    public int IndentLevel { get; set; }
    public int TabSize { get; init; } = 4;
    public IEventFormatter Formatter { get; init; }

    void Indent()
    {
        if (_inCondition)
            return;

        if (!PrettyPrint)
        {
            if (_builder.Length != _initialPos)
                _builder.Append(' ');
            return;
        }

        if (_builder.Length != _initialPos)
            _builder.AppendLine();
        _builder.Append(new string(' ', IndentLevel));
    }

    void Push() => IndentLevel += TabSize;
    void Pop() => IndentLevel -= TabSize;

    public void Visit(SingleEvent e)
    {
        Indent();
        _builder.EventScope(e.OriginalIndex, (e, this), static x =>
        {
            var visitor = x.Item2;
            if (!visitor._inCondition && visitor.PrettyPrint && visitor.Formatter != null)
            {
                // Print with a comment when the event contains a string reference
                visitor.Formatter.Format(visitor._builder, x.e.Event);
                return;
            }

            x.e.Event.Format(visitor._builder);
        });
    }

    public void Visit(BreakStatement breakStatement) { Indent(); _builder.Add(ScriptPartType.Keyword, "break"); }
    public void Visit(ContinueStatement continueStatement) { Indent(); _builder.Add(ScriptPartType.Keyword, "continue"); }
    public void Visit(ControlFlowNode cfgNode) { Indent(); _builder.Add(ScriptPartType.Error, "!!!ARBITRARY CONTROL FLOW!!!"); }
    public void Visit(EmptyNode empty) { _builder.Add(ScriptPartType.Comment, "ø"); }
    public void Visit(Name name) => _builder.Add(ScriptPartType.Identifier, name.Value);

    public void Visit(Negation negation)
    {
        _builder.Add(ScriptPartType.Operator, "!");
        bool parens = negation.Expression.Priority > negation.Priority;
        if (parens) _builder.Append('(');
        negation.Expression.Accept(this);
        if (parens) _builder.Append(')');
    }
    public void Visit(Numeric numeric) => _builder.Add(ScriptPartType.Number, numeric.Value.ToString(CultureInfo.InvariantCulture));

    public void Visit(IfThen ifThen)
    {
        Indent();
        _builder.Add(ScriptPartType.Keyword, "if");
        _builder.Append(" (");

        _inCondition = true;
        ifThen.Condition.Accept(this);
        _inCondition = false;

        _builder.Append(") {");

        Push();
        ifThen.Body?.Accept(this);
        Pop();
        Indent();

        _builder.Append("}");
    }

    public void Visit(IfThenElse ifElse)
    {
        Indent();
        _builder.Add(ScriptPartType.Keyword, "if");
        _builder.Append(" (");

        _inCondition = true;
        ifElse.Condition.Accept(this);
        _inCondition = false;

        _builder.Append(") {");

        Push();
        ifElse.TrueBody?.Accept(this);
        Pop();
        Indent();

        _builder.Append("} ");
        _builder.Add(ScriptPartType.Keyword, "else");
        _builder.Append(" {");

        Push();
        ifElse.FalseBody?.Accept(this);
        Pop();
        Indent();

        _builder.Append("}");
    }

    public void Visit(Goto jump)
    {
        Indent();
        _builder.Add(ScriptPartType.Keyword, "goto ");
        _builder.Add(ScriptPartType.Label, jump.Label);
    }

    public void Visit(Statement statement)
    {
        Indent();
        if (WrapStatements)
            _builder.Append("S(");

        statement.Head.Accept(this);
        foreach (var part in statement.Parameters)
        {
            _builder.Append(' ');
            part.Accept(this);
        }

        if (WrapStatements)
            _builder.Append(")");
    }

    public void Visit(Sequence sequence)
    {
        bool first = true;
        foreach (var node in sequence.Statements)
        {
            if (!first && !PrettyPrint)
                _builder.Append(",");
            node.Accept(this);
            first = false;
        }
    }

    public void Visit(DoLoop doLoop)
    {
        Indent();
        _builder.Add(ScriptPartType.Keyword, "do");
        _builder.Append(" {");

        Push();
        doLoop.Body?.Accept(this);
        Pop();
        Indent();

        _builder.Append("} ");
        _builder.Add(ScriptPartType.Keyword, "while");
        _builder.Append(" (");

        _inCondition = true;
        doLoop.Condition.Accept(this);
        _inCondition = false;

        _builder.Append(")");
    }

    public void Visit(EndlessLoop loop)
    {
        Indent();
        _builder.Add(ScriptPartType.Keyword, "loop");
        _builder.Append(" {");

        Push();
        loop.Body?.Accept(this);
        Pop();
        Indent();

        _builder.Append("}");
    }

    public void Visit(WhileLoop whileLoop)
    {
        Indent();
        _builder.Add(ScriptPartType.Keyword, "while");
        _builder.Append(" (");

        _inCondition = true;
        whileLoop.Condition.Accept(this);
        _inCondition = false;

        _builder.Append(") {");

        Push();
        whileLoop.Body?.Accept(this);
        Pop();
        Indent();

        _builder.Append("}");
    }

    public void Visit(Label label)
    {
        Indent();
        _builder.Add(ScriptPartType.Label, label, (l, sb) =>
        {
            sb.Append(l.Name);
            sb.Append(':');
        });
    }

    public void Visit(BinaryOp binaryOp)
    {
        bool parens = binaryOp.Left.Priority > binaryOp.Priority;
        if (parens) _builder.Append('(');
        binaryOp.Left.Accept(this);
        if (parens) _builder.Append(')');

        bool useSpaces = binaryOp.Operation is not ScriptOp.Member and not ScriptOp.BitwiseAnd and not ScriptOp.BitwiseOr;

        if(useSpaces) _builder.Append(' ');
        _builder.Add(ScriptPartType.Operator, binaryOp.Operation.ToPseudocode());
        if (useSpaces) _builder.Append(' ');

        parens = binaryOp.Right.Priority > binaryOp.Priority;
        if (parens) _builder.Append('(');
        binaryOp.Right.Accept(this);
        if (parens) _builder.Append(')');
    }
}