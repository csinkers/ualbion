using System;
using UAlbion.Api.Eventing;

namespace UAlbion.Scripting;

public record struct ScriptPart(ScriptPartType Type, Range Range);