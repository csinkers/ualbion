using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core.Events;

namespace UAlbion.Core
{
    public interface ISelectionManager
    {
        IList<Selection> CastRayFromScreenSpace(Vector2 pixelPosition, bool performFocusAlerts = false);
    }
}