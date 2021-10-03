namespace UAlbion.Formats.Scripting
{
    public interface ICondition : ICfgNode
    {
        int Precedence { get; }
    }
}