using UAlbion.Game.Diag;
using UAlbion.Game.Veldrid.Diag.Reflection;

namespace UAlbion.Game.Veldrid.Diag;

public delegate object DiagInspectorBehaviour(DebugInspectorAction action, in ReflectorState target);