using FluentAssertions;
using Hall_rent.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hall_rent.Tests.Exceptions;

public sealed class SqlErrorClassifierTests
{
    [Fact]
    public void IsUniqueViolation_ShouldReturnFalse_ForNonSqlException()
    {
        SqlErrorClassifier.IsUniqueViolation(new InvalidOperationException()).Should().BeFalse();
    }

    [Fact]
    public void IsSerializationFailure_ShouldReturnFalse_ForNonSqlException()
    {
        SqlErrorClassifier.IsSerializationFailure(new InvalidOperationException()).Should().BeFalse();
    }

    [Fact]
    public void Classifiers_ShouldReturnFalse_ForDbUpdateExceptionWithoutSqlInnerException()
    {
        var exception = new DbUpdateException("update failed", new InvalidOperationException("not sql"));

        SqlErrorClassifier.IsUniqueViolation(exception).Should().BeFalse();
        SqlErrorClassifier.IsSerializationFailure(exception).Should().BeFalse();
    }
}