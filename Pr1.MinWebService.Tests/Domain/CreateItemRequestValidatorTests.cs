using Pr1.MinWebService.Domain;
using Xunit;

namespace Pr1.MinWebService.Tests.Domain;

/// <summary>
/// Проверки логики предметной области: правила валидации отлавливают недопустимые значения.
/// </summary>
public sealed class CreateItemRequestValidatorTests
{
    [Fact]
    public void Validate_ValidRequest_ReturnsNull()
    {
        var request = new CreateItemRequest("Half-Life 2", 2004);
        var result = CreateItemRequestValidator.Validate(request);
        Assert.Null(result);
    }

    [Fact]
    public void Validate_NullName_ReturnsError()
    {
        var request = new CreateItemRequest(null!, 2020);
        var result = CreateItemRequestValidator.Validate(request);
        Assert.NotNull(result);
        Assert.Contains("name", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_EmptyName_ReturnsError()
    {
        var request = new CreateItemRequest("", 2020);
        var result = CreateItemRequestValidator.Validate(request);
        Assert.NotNull(result);
        Assert.Contains("name", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_WhitespaceName_ReturnsError()
    {
        var request = new CreateItemRequest("   ", 2020);
        var result = CreateItemRequestValidator.Validate(request);
        Assert.NotNull(result);
        Assert.Contains("name", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_NegativeYear_ReturnsError()
    {
        var request = new CreateItemRequest("Игра", -1);
        var result = CreateItemRequestValidator.Validate(request);
        Assert.NotNull(result);
        Assert.Contains("year", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_ZeroYear_ReturnsNull()
    {
        var request = new CreateItemRequest("Старая игра", 0);
        var result = CreateItemRequestValidator.Validate(request);
        Assert.Null(result);
    }

    [Fact]
    public void Validate_NullRequest_ReturnsError()
    {
        var result = CreateItemRequestValidator.Validate(null!);
        Assert.NotNull(result);
    }
}
