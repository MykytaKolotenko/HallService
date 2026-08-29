using System.Net;
using System.Text.Json;
using FluentAssertions;
using Hall_rent.Exceptions;
using Hall_rent.Exceptions.Handling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Hall_rent.Tests.Exceptions;

public sealed class ExceptionHandlingTests
{
    [Fact]
    public void AppExceptionResolver_ShouldMapAppException()
    {
        var resolver = new AppExceptionResolver();
        var exception = new NotFoundException("missing");

        resolver.CanHandle(exception).Should().BeTrue();
        var result = resolver.Resolve(exception, "GET /Hall");

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Title.Should().Be("NotFound");
        result.LogLevel.Should().Be(LogLevel.Information);
        result.Exception.Should().BeSameAs(exception);
    }

    [Fact]
    public void AppExceptionResolver_ShouldNotHandleRegularException()
    {
        new AppExceptionResolver().CanHandle(new InvalidOperationException()).Should().BeFalse();
    }

    [Fact]
    public void FallbackResolver_ShouldHandleEverythingAs500()
    {
        var resolver = new FallbackExceptionResolver();
        var exception = new InvalidOperationException("boom");

        resolver.CanHandle(exception).Should().BeTrue();
        var result = resolver.Resolve(exception, "GET /Hall");

        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Title.Should().Be("Internal Server Error");
        result.LogLevel.Should().Be(LogLevel.Error);
        result.Exception.Should().BeSameAs(exception);
    }

    [Fact]
    public void Dispatcher_ShouldUseFirstMatchingResolver()
    {
        var first = new Mock<IExceptionResolver>();
        var second = new Mock<IExceptionResolver>();
        var ex = new InvalidOperationException();
        first.Setup(x => x.CanHandle(ex)).Returns(true);
        first.Setup(x => x.Resolve(ex, "ctx"))
            .Returns(new ExceptionResolution(ex, HttpStatusCode.BadRequest, "First", LogLevel.Warning));
        second.Setup(x => x.CanHandle(ex)).Returns(true);

        var dispatcher = new ExceptionDispatcher([first.Object, second.Object]);
        var result = dispatcher.Resolve(ex, "ctx");

        result.Title.Should().Be("First");
        second.Verify(x => x.Resolve(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Dispatcher_ShouldFallbackTo500_WhenNoResolverMatches()
    {
        var resolver = new Mock<IExceptionResolver>();
        var ex = new InvalidOperationException();
        resolver.Setup(x => x.CanHandle(ex)).Returns(false);

        var result = new ExceptionDispatcher([resolver.Object]).Resolve(ex, "ctx");

        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Title.Should().Be("Internal Server Error");
        result.LogLevel.Should().Be(LogLevel.Error);
    }

    [Fact]
    public void AppExceptions_ShouldExposeExpectedStatusCodes()
    {
        new NotFoundException("x").StatusCode.Should().Be(HttpStatusCode.NotFound);
        new HallNotAvailableException(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddHours(1)).StatusCode.Should().Be(HttpStatusCode.Conflict);
        new HallCapacityExceededException(Guid.NewGuid(), 10, 11).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        new FavoursNotOfferedException(Guid.NewGuid(), [Guid.NewGuid()]).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        new HallNameAlreadyExistsException("Hall", new Exception()).StatusCode.Should().Be(HttpStatusCode.Conflict);
        new ConcurrencyConflictException("booking", new Exception()).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GlobalExceptionHandler_ShouldWriteProblemResponse()
    {
        var dispatcher = new ExceptionDispatcher([
            new AppExceptionResolver(),
            new FallbackExceptionResolver()
        ]);
        var logger = new Mock<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(dispatcher, logger.Object);
        var context = new DefaultHttpContext();
        context.TraceIdentifier = "trace-123";
        await using var body = new MemoryStream();
        context.Response.Body = body;

        var handled = await handler.TryHandleAsync(
            context,
            new NotFoundException("Hall not found"),
            CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(404);
        context.Response.ContentType.Should().StartWith("application/problem+json");
        body.Position = 0;
        using var document = await JsonDocument.ParseAsync(body);
        document.RootElement.GetProperty("title").GetString().Should().Be("NotFound");
        document.RootElement.GetProperty("status").GetInt32().Should().Be(404);
        document.RootElement.GetProperty("traceId").GetString().Should().Be("trace-123");
        document.RootElement.GetProperty("errors")[0].GetString().Should().Be("Hall not found");
    }
}