using System.Net;

namespace Hall_rent.Exceptions.Handling;

public sealed record ExceptionResolution(
    IReadOnlyList<string> Errors, // сообщения, которые пойдут в "errors" клиенту (может быть несколько — например, при валидации)
    HttpStatusCode StatusCode, // какой HTTP-статус вернуть клиенту
    string Title, // "title" в ответе ("ValidationError", "NotFound", "Internal Server Error" и т.п.)
    LogLevel LogLevel, // с каким уровнем логировать
    Exception Exception); // исходное (не обёрнутое) исключение — только для логов, клиенту не показывается
