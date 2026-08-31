using FluentAssertions;
using FluentValidation;
using Hall_rent.Controllers;
using Hall_rent.Dto;
using Hall_rent.Request;
using Hall_rent.Response;
using Hall_rent.Service.Interface;
using Hall_rent.Validation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Hall_rent.Tests.Controllers;

public class BookingControllerTest
{
    private readonly Mock<IBookingService> _bookingService = new Mock<IBookingService>();

    private BookingController Sut()
    {
        return new BookingController(
            _bookingService.Object,
            new HallBookRequestValidator(new FixedClock(DateTime.UtcNow)));
    }

    [Fact]
    public async Task BookHall_ShouldRejectInvalidBodyBeforeService()
    {
        var request = new HallBookRequest
        {
            From = DateTime.UtcNow.AddMinutes(-5),
            To = DateTime.UtcNow.AddMinutes(-1),
            Persons = 0,
            Favors = []
        };

        var act = () => Sut().BookHall(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<ValidationException>();
        _bookingService.Verify(x => x.BookAsync(It.IsAny<BookHallDto>()), Times.Never);
    }

    [Fact]
    public async Task BookHall_ShouldPassRouteAndBodyAndReturnOk()
    {
        var hallId = Guid.NewGuid();
        var favorId = Guid.NewGuid();
        var request = new HallBookRequest
        {
            From = DateTime.UtcNow.AddDays(1),
            To = DateTime.UtcNow.AddDays(1).AddHours(2),
            Persons = 10,
            Favors = [favorId]
        };
        var response = new HallBookResponse { Id = Guid.NewGuid(), Price = 150m };
        _bookingService.Setup(x => x.BookAsync(It.IsAny<BookHallDto>())).ReturnsAsync(response);

        var result = await Sut().BookHall(hallId, request);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(response);
        _bookingService.Verify(x => x.BookAsync(It.Is<BookHallDto>(d =>
            d.HallId == hallId &&
            d.Persons == 10 &&
            d.Favors.SequenceEqual(new List<Guid> { favorId }) &&
            d.StartAt == request.From &&
            d.EndAt == request.To)), Times.Once);
    }
}