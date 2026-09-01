namespace Hall_rent.Exceptions.Handling;

// Catches the raw SqlException/DbUpdateException with serialization failure code,
public class SerializationConflictResolver : IExceptionResolver
{
    public bool CanHandle(Exception ex)
    {
        return SqlErrorClassifier.IsSerializationFailure(ex);
    }

    public ExceptionResolution Resolve(Exception ex, string context)
    {
        var mapped = new ConcurrencyConflictException(context, ex);
        return new ExceptionResolution([mapped.Message], mapped.StatusCode, mapped.Title, mapped.LogLevel, mapped);
    }
}
