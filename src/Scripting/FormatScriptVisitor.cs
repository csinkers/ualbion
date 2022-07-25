using System;
using System.Collections.Generic;
using System.Text;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting;

public class FormatScriptVisitor : IAstVisitor
{
    readonly StringBuilder _sb;
    readonly Dictionary<int, (int start, int end)> _mapping;
    readonly long _initialPos;
    bool _inCondition;

    public FormatScriptVisitor() { _sb = new StringBuilder(); }
    public FormatScriptVisitor(StringBuilder sb, Dictionary<int, (int start, int end)> mapping)
    {
        _sb = sb ?? throw new ArgumentNullException(nameof(sb));
        _mapping = mapping;
        _initialPos = _sb.Length;
    }

    public string Code => _sb.ToString();
    public bool UseNumericIds { get; init; }
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
            if (_sb.Length != _initialPos)
                _sb.Append(' ');
            return;
        }

        if (_sb.Length != _initialPos)
            _sb.AppendLine();
        _sb.Append(new string(' ', IndentLevel));
    }

    void Push() => IndentLevel += TabSize;
    void Pop() => IndentLevel -= TabSize;

    public void Visit(SingleEvent e)
    {
        Indent();
        int startPosition = _sb.Length;
        if (!_inCondition && PrettyPrint && Formatter != null)
        {
            _sb.Append(Formatter.Format(e.Event, UseNumericIds));
            if (_mapping != null)
                _mapping[e.OriginalIndex] = (startPosition, _sb.Length);
            return;
        }

        _sb.Append(UseNumericIds 
            ? e.Event.ToStringNumeric() 
            : e.Event.ToString());

        if (_mapping != null)
            _mapping[e.OriginalIndex] = (startPosition, _sb.Length);
    }

    public void Visit(BreakStatement breakStatement) { Indent(); _sb.Append("break"); }
    public void Visit(ContinueStatement continueStatement) { Indent(); _sb.Append("continue"); }
    public void Visit(ControlFlowNode cfgNode) { Indent(); _sb.Append("!!!ARBITRARY CONTROL FLOW!!!"); }
    public void Visit(EmptyNode empty) { _sb.Append('ø'); }
    public void Visit(Name name) => _sb.Append(name.Value);

    public void Visit(Negation negation)
    {
        _sb.Append('!');
        bool parens = negation.Expression.Priority > negation.Priority;
        if (parens) _sb.Append('(');
        negation.Expression.Accept(this);
        if (parens) _sb.Append(')');
    }
    public void Visit(Numeric numeric) => _sb.Append(numeric.Value);

    public void Visit(IfThen ifThen)
    {
        Indent();
        _sb.Append("if (");
        _inCondition = true;
        ifThen.Condition.Accept(this);
        _inCondition = false;
        _sb.Append(") {");
        Push();
        ifThen.Body?.Accept(this);
        Pop();
        Indent();
        _sb.Append("}");
    }

    public void Visit(IfThenElse ifElse)
    {
        Indent();
        _sb.Append("if (");
        _inCondition = true;
        ifElse.Condition.Accept(this);
        _inCondition = false;
        _sb.Append(") {");
        Push();
        ifElse.TrueBody?.Accept(this);
        Pop();
        Indent();
        _sb.Append("} else {");
        Push();
        ifElse.FalseBody?.Accept(this);
        Pop();
        Indent();
        _sb.Append("}");
    }

    public void Visit(Goto jump)
    {
        Indent();
        _sb.Append("goto ");
        _sb.Append(jump.Label);
    }

    public void Visit(Statement statement)
    {
        Indent();
        if (WrapStatements)
            _sb.Append("S(");

        statement.Head.Accept(this);
        foreach (var part in statement.Parameters)
        {
            _sb.Append(' ');
            part.Accept(this);
        }

        if (WrapStatements)
            _sb.Append(")");
    }

    public void Visit(Sequence sequence)
    {
        bool first = true;
        foreach (var node in sequence.Statements)
        {
            if (!first && !PrettyPrint)
                _sb.Append(",");
            node.Accept(this);
            first = false;
        }
    }

    public void Visit(DoLoop doLoop)
    {
        Indent();
        _sb.Append("do {");
        Push();
        doLoop.Body?.Accept(this);
        Pop();
        Indent();
        _sb.Append("} while (");
        _inCondition = true;
        doLoop.Condition.Accept(this);
        _inCondition = false;
        _sb.Append(")");
    }

    public void Visit(EndlessLoop loop)
    {
        Indent();
        _sb.Append("loop {");
        Push();
        loop.Body?.Accept(this);
        Pop();
        Indent();
        _sb.Append("}");
    }

    public void Visit(WhileLoop whileLoop)
    {
        Indent();
        _sb.Append("while (");
        _inCondition = true;
        whileLoop.Condition.Accept(this);
        _inCondition = false;
        _sb.Append(") {");
        Push();
        whileLoop.Body?.Accept(this);
        Pop();
        Indent();
        _sb.Append("}");
    }

    public void Visit(Label label)
    {
        Indent();
        _sb.Append(label.Name);
        _sb.Append(":");
    }

    public void Visit(BinaryOp binaryOp)
    {
        bool parens = binaryOp.Left.Priority > binaryOp.Priority;
        if (parens) _sb.Append('(');
        binaryOp.Left.Accept(this);
        if (parens) _sb.Append(')');

        if(binaryOp.Operation != ScriptOp.Member) _sb.Append(' ');
        _sb.Append(binaryOp.Operation.ToPseudocode());
        if (binaryOp.Operation != ScriptOp.Member) _sb.Append(' ');

        parens = binaryOp.Right.Priority > binaryOp.Priority;
        if (parens) _sb.Append('(');
        binaryOp.Right.Accept(this);
        if (parens) _sb.Append(')');
    }
}