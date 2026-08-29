using FluentAssertions;
using FluentValidation.TestHelper;
using Hall_rent.Request;
using Xunit;

namespace Hall_rent.Tests.Validators;

public sealed class FavorUpdateRequestValidatorTests
{
    private readonly FavorUpdateRequestValidator _validator = new();

    [Fact]
    public void ShouldAcceptValidRequest()
    {
        _validator.Validate(new FavorUpdateRequest { Name = "Parking", Price = 20m })
            .IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    public void ShouldRejectNonPositivePrice(decimal price)
    {
        _validator.TestValidate(new FavorUpdateRequest { Name = "Parking", Price = price })
            .ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void ShouldRejectEmptyName()
    {
        _validator.TestValidate(new FavorUpdateRequest { Name = "", Price = 20m })
            .ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldRejectNameLongerThan150Characters()
    {
        _validator.TestValidate(new FavorUpdateRequest { Name = new string('x', 151), Price = 20m })
            .ShouldHaveValidationErrorFor(x => x.Name);
    }
}