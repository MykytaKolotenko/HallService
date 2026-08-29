using FluentAssertions;
using FluentValidation.TestHelper;
using Hall_rent.Request;
using Hall_rent.Validation;
using Xunit;

namespace Hall_rent.Tests.Validators;

public sealed class HallCreateRequestValidatorTests
{
    private readonly HallCreateRequestValidator _validator = new();

    [Fact]
    public void ShouldAcceptValidRequest()
    {
        _validator.Validate(new HallCreateRequest { Name = "Main Hall", Persons = 20, Price = 100m })
            .IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldRejectEmptyName()
    {
        _validator.TestValidate(new HallCreateRequest { Name = "", Persons = 20, Price = 100m })
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Hall name is required.");
    }

    [Fact]
    public void ShouldRejectNameLongerThan200Characters()
    {
        _validator.TestValidate(new HallCreateRequest { Name = new string('x', 201), Persons = 20, Price = 100m })
            .ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ShouldRejectNonPositiveCapacity(int persons)
    {
        _validator.TestValidate(new HallCreateRequest { Name = "Hall", Persons = persons, Price = 100m })
            .ShouldHaveValidationErrorFor(x => x.Persons);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ShouldRejectNonPositivePrice(decimal price)
    {
        _validator.TestValidate(new HallCreateRequest { Name = "Hall", Persons = 10, Price = price })
            .ShouldHaveValidationErrorFor(x => x.Price);
    }
}