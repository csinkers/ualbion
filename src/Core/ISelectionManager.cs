using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core.Events;

namespace UAlbion.Core;

public interface ISelectionManager
{
    void CastRayFromScreenSpace(List<Selection> hits, Vector2 pixelPosition, bool debug, bool performFocusAlerts);
}