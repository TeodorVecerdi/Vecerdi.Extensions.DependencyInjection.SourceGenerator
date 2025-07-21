namespace UnityEngine;

[AttributeUsage(AttributeTargets.Class)]
public sealed class DefaultExecutionOrderAttribute(int order) : Attribute {
    public int Order { get; } = order;
}
