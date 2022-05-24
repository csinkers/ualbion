using System;

namespace UAlbion.Core.Visual;

public class ShaderInfo
{
    public ShaderInfo(string name, string content)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(name);
        if (string.IsNullOrEmpty(content)) throw new ArgumentNullException(content);
        Name = name;
        Content = content;
    }

    public string Name { get; }
    public string Content { get; }
}