using System;

namespace UAlbion.Api.Eventing;

/// <summary>
/// Used to specify which constructor should be used when parsing an event from text using its reflection metadata.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public sealed class EventConstructorAttribute : Attribute
{
}