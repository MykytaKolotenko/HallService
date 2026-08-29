using FluentAssertions;
using FluentValidation.TestHelper;
using Hall_rent.Request;
using Hall_rent.Validation;
using Xunit;

namespace Hall_rent.Tests.Validators;

public sealed class FavorCreateRequestValidatorTests
{
    private readonly FavorCreateRequestValidator _validator = new();

    [Fact]
    public void ShouldAcceptValidRequest()
    {
        var result = _validator.Validate(new FavorCreateRequest { Name = "Projector", Price = 10m });
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ShouldRejectNonPositivePrice(decimal price)
    {
        var result = _validator.TestValidate(new FavorCreateRequest { Name = "Projector", Price = price });
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("Price must be greater than 0.");
    }

    [Fact]
    public void ShouldRejectEmptyName()
    {
        _validator.TestValidate(new FavorCreateRequest { Name = "", Price = 10m })
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Favor name is required.");
    }

    [Fact]
    public void ShouldRejectNameLongerThan150Characters()
    {
        var name = new string('a', 151);
        _validator.TestValidate(new FavorCreateRequest { Name = name, Price = 10m })
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Favor name must not exceed 150 characters.");
    }

    [Fact]
    public void ShouldAcceptNameExactly150Characters()
    {
        var name = new string('a', 150);
        _validator.Validate(new FavorCreateRequest { Name = name, Price = 10m })
            .IsValid.Should().BeTrue();
    }
}