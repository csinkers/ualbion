using System;
using System.Collections.Generic;

namespace UAlbion.Api.Settings;

public interface IVarRegistry
{
    public IEnumerable<IVar> Vars { get; }

    bool IsVarRegistered(string key);
    void Register(Type type);
    void Clear();
}