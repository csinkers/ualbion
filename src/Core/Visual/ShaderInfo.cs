using System;
using System.Text;
using UAlbion.Api;

namespace UAlbion.Core.Visual;

public class ShaderInfo
{
    public ShaderInfo(string name, string content)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(name);
        if (string.IsNullOrEmpty(content)) throw new ArgumentNullException(content);
        Name = name;
        Content = content;
        Hash = HashUtil.Fnv1A(Encoding.UTF8.GetBytes(Content)).ToString("X8");
    }

    public string Name { get; }
    public string Content { get; }
    public string Hash { get; }
    public override string ToString() => $"{Name}.{Hash}";
}
