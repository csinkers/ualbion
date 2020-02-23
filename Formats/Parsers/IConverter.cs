namespace UAlbion.Formats.Parsers
{
    public interface IConverter<TPersistent, TMemory>
    {
        TPersistent ToPersistent(TMemory memory);
        TMemory ToMemory(TPersistent persistent);
    }
}