using System.Net;
using FluentAssertions;
using Hall_rent.Exceptions;
using Hall_rent.Exceptions.Handling;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Hall_rent.Tests.Exceptions;

public sealed class ExceptionHandlingTests
{
    [Fact]
    public void AppExceptionResolver_ShouldMapAppException()
    {
        AppExceptionResolver resolver = new AppExceptionResolver();
        NotFoundException exception = new NotFoundException("missing");

        resolver.CanHandle(exception).Should().BeTrue();
        ExceptionResolution result = resolver.Resolve(exception, "GET /Hall");

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
        FallbackExceptionResolver resolver = new FallbackExceptionResolver();
        InvalidOperationException exception = new InvalidOperationException("boom");

        resolver.CanHandle(exception).Should().BeTrue();
        ExceptionResolution result = resolver.Resolve(exception, "GET /Hall");

        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Title.Should().Be("Internal Server Error");
        result.LogLevel.Should().Be(LogLevel.Error);
        result.Exception.Should().BeSameAs(exception);
    }

    [Fact]
    public void Dispatcher_ShouldUseFirstMatchingResolver()
    {
        Mock<IExceptionResolver> first = new Mock<IExceptionResolver>();
        Mock<IExceptionResolver> second = new Mock<IExceptionResolver>();
        InvalidOperationException ex = new InvalidOperationException();
        first.Setup(x => x.CanHandle(ex)).Returns(true);
        first.Setup(x => x.Resolve(ex, "ctx"))
            .Returns(new ExceptionResolution(new List<string> { ex.Message }, HttpStatusCode.BadRequest, "First", LogLevel.Warning, ex));
        second.Setup(x => x.CanHandle(ex)).Returns(true);

        ExceptionDispatcher dispatcher = new ExceptionDispatcher([first.Object, second.Object]);
        ExceptionResolution result = dispatcher.Resolve(ex, "ctx");

        result.Title.Should().Be("First");
        second.Verify(x => x.Resolve(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Dispatcher_ShouldFallbackTo500_WhenNoResolverMatches()
    {
        Mock<IExceptionResolver> resolver = new Mock<IExceptionResolver>();
        InvalidOperationException ex = new InvalidOperationException();
        resolver.Setup(x => x.CanHandle(ex)).Returns(false);

        ExceptionResolution result = new ExceptionDispatcher([resolver.Object]).Resolve(ex, "ctx");

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
        new FavorsNotOfferedException(Guid.NewGuid(), [Guid.NewGuid()]).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        new HallNameAlreadyExistsException("Hall", new Exception()).StatusCode.Should().Be(HttpStatusCode.Conflict);
        new ConcurrencyConflictException("booking", new Exception()).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}