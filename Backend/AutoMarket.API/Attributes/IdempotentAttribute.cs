namespace AutoMarket.API.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class IdempotentAttribute : Attribute { }
