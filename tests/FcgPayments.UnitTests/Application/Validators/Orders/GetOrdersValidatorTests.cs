using FcgPayments.Application.Queries.Orders;
using FcgPayments.Application.Validators.Orders;
using Shouldly;

namespace FcgPayments.UnitTests.Application.Validators.Orders;

public class GetOrdersValidatorTests
{
    private readonly GetOrdersValidator _validator;

    public GetOrdersValidatorTests()
    {
        _validator = new GetOrdersValidator();
    }

    [Fact]
    public void Dado_ConsultaValida_Quando_Validar_Entao_NaoRetornaErros()
    {
        // Arrange
        var query = CreateValidQuery();

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    // Page
    [Fact]
    public void Dado_PageIgualZero_Quando_Validar_Entao_RetornaErro()
    {
        // Arrange
        var query = CreateValidQuery() with { Page = 0 };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == "Page");
    }

    [Fact]
    public void Dado_PageNegativa_Quando_Validar_Entao_RetornaErro()
    {
        // Arrange
        var query = CreateValidQuery() with { Page = -1 };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
    }

    // PageSize
    [Fact]
    public void Dado_PageSizeZero_Quando_Validar_Entao_RetornaErro()
    {
        // Arrange
        var query = CreateValidQuery() with { PageSize = 0 };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Dado_PageSizeMaiorQue100_Quando_Validar_Entao_RetornaErro()
    {
        // Arrange
        var query = CreateValidQuery() with { PageSize = 101 };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Dado_PageSizeMaximoPermitido_Quando_Validar_Entao_Aceita()
    {
        // Arrange
        var query = CreateValidQuery() with { PageSize = 100 };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    private static GetOrdersQuery CreateValidQuery()
        => new()
        {
            Page = 1,
            PageSize = 10,
            UserId = "user-1",
            IsAdmin = false
        };
}
