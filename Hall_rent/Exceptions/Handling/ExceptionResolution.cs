using System.Net;

namespace Hall_rent.Exceptions.Handling;

public record ExceptionResolution(
    Exception Exception, // что пробрасывать наружу / что попадёт в лог и в detail ответа
    HttpStatusCode StatusCode, // какой HTTP-статус вернуть клиенту (в middleware)
    string Title, // короткое имя ошибки для problem+json ("Not Found", "Conflict" и т.п.)
    LogLevel LogLevel); // с каким уровнем логировать (Information для ожидаемых бизнес-ошибок, Error для непредвиденных)