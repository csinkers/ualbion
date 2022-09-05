using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core.Events;

namespace UAlbion.Game.Gui;

#pragma warning disable CA2227 // Collection properties should be read only - for perf reasons
public class SelectionContext
{
    public Vector2 UiPosition { get; set; }
    public Vector2 NormPosition { get; set; }
    public List<Selection> Selections { get; set; }
    public void AddHit(int order, object target)
    {
        float z = 1.0f - order / (float)DrawLayer.MaxLayer;
        var intersectionPoint = new Vector3(NormPosition, z);
        Selections.Add(new Selection(intersectionPoint, z, target));
    }
}
#pragma warning restore CA2227 // Collection properties should be read only