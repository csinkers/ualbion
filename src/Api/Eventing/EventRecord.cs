namespace UAlbion.Api.Eventing;

public abstract record EventRecord : IEvent
{
    public override string ToString() => EventSerializer.Instance.ToString(this);
    public void Format(IScriptBuilder builder) => EventSerializer.Instance.Format(builder, this);
}