namespace Hall_rent.Exceptions.Handling;

// Ловит "сырую" SqlException/DbUpdateException с кодом serialization failure (1205/3960),
// которая долетела до middleware необёрнутой (например, из места, где явного catch не было),
// и превращает её в понятный клиенту конфликт.
public class SerializationConflictResolver : IExceptionResolver
{
    public bool CanHandle(Exception ex) => SqlErrorClassifier.IsSerializationFailure(ex);

    public ExceptionResolution Resolve(Exception ex, string context)
    {
        var mapped = new ConcurrencyConflictException(context, ex);
        return new ExceptionResolution([mapped.Message], mapped.StatusCode, mapped.Title, mapped.LogLevel, mapped);
    }
}