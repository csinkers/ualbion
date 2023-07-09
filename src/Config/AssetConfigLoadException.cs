using System;
using System.Runtime.Serialization;

namespace UAlbion.Config;

[Serializable]
public class AssetConfigLoadException : Exception
{
    public AssetConfigLoadException() { }
    public AssetConfigLoadException(string message) : base(message) { }
    public AssetConfigLoadException(string message, Exception exception) : base(message, exception) { }
    protected AssetConfigLoadException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}