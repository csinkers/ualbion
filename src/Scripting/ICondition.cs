namespace UAlbion.Scripting
{
    public interface ICondition : ICfgNode
    {
        int Precedence { get; }
    }
}