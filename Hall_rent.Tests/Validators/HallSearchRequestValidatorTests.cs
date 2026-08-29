using FluentAssertions;
using FluentValidation.TestHelper;
using Hall_rent.Request;
using Xunit;

namespace Hall_rent.Tests.Validators;

public sealed class HallSearchRequestValidatorTests
{
    private readonly DateTime _now = new(2030, 1, 10, 12, 0, 0, DateTimeKind.Utc);
    private HallSearchRequestValidator Validator() => new(new FixedClock(_now));

    [Fact]
    public void ShouldAcceptValidFutureInterval()
    {
        var request = new HallSearchRequest
        {
            StartAt = _now.AddHours(1),
            EndAt = _now.AddHours(2),
            Persons = 10
        };

        Validator().Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldRejectStartAtAfterEndAt()
    {
        var request = new HallSearchRequest
        {
            StartAt = _now.AddHours(3),
            EndAt = _now.AddHours(2),
            Persons = 10
        };

        Validator().TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.StartAt);
    }

    [Fact]
    public void ShouldRejectEndAtInThePast()
    {
        var request = new HallSearchRequest
        {
            StartAt = _now.AddHours(-3),
            EndAt = _now.AddHours(-2),
            Persons = 10
        };

        Validator().TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.EndAt);
    }

    [Fact]
    public void ShouldRejectZeroPersons()
    {
        var request = new HallSearchRequest
        {
            StartAt = _now.AddHours(1),
            EndAt = _now.AddHours(2),
            Persons = 0
        };

        Validator().TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Persons);
    }
}