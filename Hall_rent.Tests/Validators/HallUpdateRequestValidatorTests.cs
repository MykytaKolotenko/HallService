using FluentAssertions;
using FluentValidation.TestHelper;
using Hall_rent.Request;
using Hall_rent.Validation;
using Xunit;

namespace Hall_rent.Tests.Validators;

public sealed class HallUpdateRequestValidatorTests
{
    private readonly HallUpdateRequestValidator _validator = new();

    [Fact]
    public void ShouldAcceptValidRequest()
    {
        _validator.Validate(new HallUpdateRequest
        {
            Name = "Main Hall",
            Persons = 20,
            Price = 100m,
            Favors = [Guid.NewGuid()]
        }).IsValid.Should().BeTrue();
    }

    [Fact]
    public void ShouldRejectEmptyName()
    {
        _validator.TestValidate(new HallUpdateRequest { Name = "", Persons = 20, Price = 100m, Favors = [] })
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Hall name is required.");
    }

    [Fact]
    public void ShouldRejectNameLongerThan200Characters()
    {
        _validator.TestValidate(new HallUpdateRequest { Name = new string('x', 201), Persons = 20, Price = 100m, Favors = [] })
            .ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldRejectNullFavors()
    {
        _validator.TestValidate(new HallUpdateRequest { Persons = 20, Price = 100m, Favors = null })
            .ShouldHaveValidationErrorFor(x => x.Favors)
            .WithErrorMessage("Favors collection cannot be null.");
    }

    [Fact]
    public void ShouldRejectEmptyFavorId()
    {
        _validator.Validate(new HallUpdateRequest { Persons = 20, Price = 100m, Favors = [Guid.Empty] })
            .IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ShouldRejectInvalidPersons(int persons)
    {
        _validator.TestValidate(new HallUpdateRequest { Persons = persons, Price = 100m, Favors = [] })
            .ShouldHaveValidationErrorFor(x => x.Persons);
    }

    [Fact]
    public void ShouldRejectNonPositivePrice()
    {
        _validator.TestValidate(new HallUpdateRequest { Persons = 10, Price = 0m, Favors = [] })
            .ShouldHaveValidationErrorFor(x => x.Price);
    }
}