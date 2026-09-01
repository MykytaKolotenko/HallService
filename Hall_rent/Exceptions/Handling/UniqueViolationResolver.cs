namespace Hall_rent.Exceptions.Handling;

public class UniqueViolationResolver : IExceptionResolver
{
    public bool CanHandle(Exception ex)
    {
        return SqlErrorClassifier.IsUniqueViolation(ex);
    }

    public ExceptionResolution Resolve(Exception ex, string context)
    {
        var mapped = UniqueConstraintExceptionFactory.Create(ex);
        return new ExceptionResolution([mapped.Message], mapped.StatusCode, mapped.Title, mapped.LogLevel, mapped);
    }
}