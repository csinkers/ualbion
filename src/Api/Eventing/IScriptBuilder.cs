using System;
using System.Text;

namespace UAlbion.Api.Eventing;

public interface IScriptBuilder
{
    public bool UseNumericIds { get; }
    public int Length { get; }
    void Append(char c);
    void Append(string text);
    void Append(object value);
    void AppendLine();
    void AppendLine(string text);
    void Add(ScriptPartType type, char c);
    void Add(ScriptPartType type, string text);
    void Add<T>(ScriptPartType type, T context, Action<T, StringBuilder> func);
    void EventScope<T>(int eventId, T context, Action<T> func);
}