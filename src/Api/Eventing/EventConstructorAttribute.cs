using System;

namespace UAlbion.Api.Eventing;

[AttributeUsage(AttributeTargets.Constructor)]
public sealed class EventConstructorAttribute : Attribute
{
}