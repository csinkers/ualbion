using System;

namespace UAlbion.Api.Settings;

public interface IVarRegistry
{
    bool IsVarRegistered(string key);
    void Register(Type type);
    void Clear();
}