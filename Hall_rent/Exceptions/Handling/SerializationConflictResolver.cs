namespace Hall_rent.Exceptions.Handling;

// Ловит "сырую" SqlException/DbUpdateException с кодом serialization failure (1205/3960),
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