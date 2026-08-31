using FluentAssertions;
using FluentValidation.TestHelper;
using Hall_rent.Request;
using Hall_rent.Validation;
using Xunit;

namespace Hall_rent.Tests.Validators;

public sealed class HallBookRequestValidatorTests
{
    private readonly HallBookRequestValidator _validator = new HallBookRequestValidator(new FixedClock(DateTime.UtcNow));

    private static HallBookRequest Valid()
    {
        return new HallBookRequest
        {
            StartAt = DateTime.UtcNow.AddHours(2),
            EndAt = DateTime.UtcNow.AddHours(4),
            Persons = 5,
            Favors = []
        };
    }

    [Fact]
    public void ShouldAcceptValidRequest()
    {
        _validator.Validate(Valid()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldRejectStartAtInThePast()
    {
        var request = Valid();
        request.StartAt = DateTime.UtcNow.AddMinutes(-1);

        _validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.StartAt)
            .WithErrorMessage("StartAt must be in the future.");
    }

    [Fact]
    public void ShouldRejectEndAtBeforeStartAt()
    {
        var request = Valid();
        request.EndAt = request.StartAt.AddMinutes(-1);

        _validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.EndAt)
            .WithErrorMessage("EndAt must be greater than StartAt.");
    }

    [Fact]
    public void ShouldRejectZeroPersons()
    {
        var request = Valid();
        request.Persons = 0;

        _validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Persons);
    }

    [Fact]
    public void ShouldRejectEmptyFavorId()
    {
        var request = Valid();
        request.Favors = [Guid.Empty];

        _validator.Validate(request).IsValid.Should().BeFalse();
    }

    [Fact]
    public void ShouldAcceptNoFavors()
    {
        _validator.Validate(Valid()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldNotThrow_WhenFavorsAreNull()
    {
        var request = Valid();
        request.Favors = null!;

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}