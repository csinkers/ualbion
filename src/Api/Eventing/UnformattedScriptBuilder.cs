using System;
using System.Text;

namespace UAlbion.Api.Eventing;

public class UnformattedScriptBuilder : IScriptBuilder
{
    readonly StringBuilder _sb = new();

    public UnformattedScriptBuilder(bool useNumericIds) => UseNumericIds = useNumericIds;
    public bool UseNumericIds { get; }
    public int Length => _sb.Length;
    public void Clear() => _sb.Clear();
    public string Build() => _sb.ToString();
    public override string ToString() => _sb.ToString();
    public void Append(char c) => _sb.Append(c);
    public void Append(string text) => _sb.Append(text);
    public void Append(object value) => _sb.Append(value);
    public void AppendLine() => _sb.AppendLine();
    public void AppendLine(string text) => _sb.AppendLine(text);
    public void Add(ScriptPartType type, char c) => _sb.Append(c);
    public void Add(ScriptPartType type, string text) => _sb.Append(text);
    public void Add<T>(ScriptPartType type, T context, Action<T, StringBuilder> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        func(context, _sb);
    }

    public void EventScope<T>(int eventId, T context, Action<T> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        func(context);
    }
}
