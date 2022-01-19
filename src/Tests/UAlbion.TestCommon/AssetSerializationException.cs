using System;

namespace UAlbion.TestCommon;

public sealed class AssetSerializationException : Exception
{
    public string Annotation { get; }
    public AssetSerializationException(Exception exception, string annotation) : base(exception.Message, exception) => Annotation = annotation;
}