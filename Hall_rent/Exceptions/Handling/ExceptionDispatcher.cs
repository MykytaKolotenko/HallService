using System.Net;

namespace Hall_rent.Exceptions.Handling;

// Chain of Responsibility: iterate through resolvers in order and take the first one that agrees
// to handle the exception (CanHandle == true). THE ORDER OF THE LIST MATTERS and is defined
// during registration in InfrastructureDi.AddExceptions — for example, ValidationExceptionResolver
// and SerializationConflictResolver/UniqueViolationResolver must come before AppExceptionResolver,
// while FallbackExceptionResolver (CanHandle is always true) must be last, otherwise it will
// "swallow" all exceptions and more specific resolvers will never be reached. It is used as a singleton,
// so resolvers themselves must not store request-specific state.
public class ExceptionDispatcher
{
    private readonly IReadOnlyList<IExceptionResolver> _handlers;

    public ExceptionDispatcher(IReadOnlyList<IExceptionResolver> handlers)
    {
        _handlers = handlers;
    }

    public ExceptionResolution Resolve(Exception ex, string context = "")
    {
        var handler = _handlers.FirstOrDefault(h => h.CanHandle(ex));

        // This fallback inside Resolve is a second, defensive safety net in case the handler list
// does not contain FallbackExceptionResolver at all (for example, due to an incorrect DI configuration).
// In the normal setup (see InfrastructureDi), this branch is never reached.
        return handler?.Resolve(ex, context)
               ?? new ExceptionResolution(
                   ["Internal Server Error"],
                   HttpStatusCode.InternalServerError,
                   "Internal Server Error",
                   LogLevel.Error,
                   ex);
    }
}