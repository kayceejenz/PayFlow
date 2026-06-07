namespace PayFlow.Shared.Primitives;

public abstract record StronglyTypedId<T>(T Value) where T : IEquatable<T>
{
    public override string ToString() => Value.ToString() ?? string.Empty;
}
