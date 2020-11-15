using SerdesNet;

namespace UAlbion.Formats.Containers
{
    public interface IFileContainer
    {
        ISerializer Open(string file, string subItem);
    }
}