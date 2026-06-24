using FcgPayments.Application.Queries.Orders;
using FcgPayments.Application.Validators.Orders;
using Shouldly;

namespace FcgPayments.UnitTests.Application.Validators.Orders;

public class GetOrderByIdValidatorTests
{
    private readonly GetOrderByIdValidator _validator;

    public GetOrderByIdValidatorTests()
    {
        _validator = new GetOrderByIdValidator();
    }

    [Fact]
    public void Dado_ComandoValido_Quando_Validar_Entao_NaoRetornaErros()
    {
        // Arrange
        var query = CreateValidQuery();

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Dado_IdVazio_Quando_Validar_Entao_RetornaErro()
    {
        // Arrange
        var query = CreateValidQuery() with { Id = Guid.Empty };

        // Act
        var result = _validator.Validate(query);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == "Id");
    }

    private static GetOrderByIdQuery CreateValidQuery()
        => new(Guid.NewGuid())
        {
            UserId = "user-1",
            IsAdmin = false
        };
}
