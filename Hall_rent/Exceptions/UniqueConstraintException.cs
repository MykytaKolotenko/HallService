public sealed class UniqueConstraintException : Exception
{
    public UniqueConstraintException(
        string constraint,
        Exception inner)
        : base(
            $"Unique constraint '{constraint}' was violated.",
            inner)
    {
        Constraint = constraint;
    }

    public string Constraint { get; }
}