using System;

namespace UAlbion.Api;

public interface IJsonUtil
{
    T Deserialize<T>(ReadOnlySpan<byte> bytes);
    T Deserialize<T>(string json);
    string Serialize<T>(T input);
}