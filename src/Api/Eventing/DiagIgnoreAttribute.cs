using System;

namespace UAlbion.Api.Eventing;

/// <summary>
/// Don't show this member in the diag inspector
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DiagIgnoreAttribute : Attribute { }