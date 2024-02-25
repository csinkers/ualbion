using System.Collections.Generic;

namespace UAlbion.Api.Settings;

public class VarLibrary
{
    public List<IVar> Vars { get; } = new();
    public void Add(IVar v) => Vars.Add(v);
}