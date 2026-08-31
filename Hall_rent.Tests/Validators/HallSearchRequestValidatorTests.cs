using FluentAssertions;
using FluentValidation.TestHelper;
using Hall_rent.Request;
using Hall_rent.Validation;
using Xunit;

namespace Hall_rent.Tests.Validators;

public sealed class HallSearchRequestValidatorTests
{
    private readonly DateTime _now = new DateTime(2030, 1, 10, 12, 0, 0, DateTimeKind.Utc);

    private HallSearchRequestValidator Validator()
    {
        return new HallSearchRequestValidator(new FixedClock(_now));
    }

    [Fact]
    public void ShouldAcceptValidFutureInterval()
    {
        var request = new HallSearchRequest
        {
            From = _now.AddHours(1),
            To = _now.AddHours(2),
            Persons = 10
        };

        Validator().Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldRejectStartAtAfterEndAt()
    {
        var request = new HallSearchRequest
        {
            From = _now.AddHours(3),
            To = _now.AddHours(2),
            Persons = 10
        };

        Validator().TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.From);
    }

    [Fact]
    public void ShouldRejectEndAtInThePast()
    {
        var request = new HallSearchRequest
        {
            From = _now.AddHours(-3),
            To = _now.AddHours(-2),
            Persons = 10
        };

        Validator().TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.To);
    }

    [Fact]
    public void ShouldRejectZeroPersons()
    {
        var request = new HallSearchRequest
        {
            From = _now.AddHours(1),
            To = _now.AddHours(2),
            Persons = 0
        };

        Validator().TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Persons);
    }
}