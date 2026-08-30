namespace Hall_rent.Exceptions.Handling;

// Симметрично SerializationConflictResolver, но для нарушения уникального индекса (2601/2627).
public class UniqueViolationResolver : IExceptionResolver
{
    public bool CanHandle(Exception ex) => SqlErrorClassifier.IsUniqueViolation(ex);

    public ExceptionResolution Resolve(Exception ex, string context)
    {
        var mapped = new UniqueConstraintException(context, ex);
        return new ExceptionResolution([mapped.Message], mapped.StatusCode, mapped.Title, mapped.LogLevel, mapped);
    }
}