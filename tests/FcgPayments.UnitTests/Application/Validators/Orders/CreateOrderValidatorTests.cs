using FcgPayments.Application.Commands.Orders;
using FcgPayments.Application.Validators.Orders;
using Shouldly;

namespace FcgPayments.UnitTests.Application.Validators.Orders;

public class CreateOrderValidatorTests
{
    private readonly CreateOrderValidator _validator;

    public CreateOrderValidatorTests()
    {
        _validator = new CreateOrderValidator();
    }

    [Fact]
    public void Dado_ComandoValido_Quando_Validar_Entao_NaoRetornaErros()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    // Customer
    [Fact]
    public void Dado_ClienteVazio_Quando_Validar_Entao_RetornaErro()
    {
        // Arrange
        var command = CreateValidCommand() with { Customer = "" };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == "Customer");
    }

    [Fact]
    public void Dado_ClienteMuitoGrande_Quando_Validar_Entao_RetornaErro()
    {
        // Arrange
        var command = CreateValidCommand() with { Customer = new string('a', 101) };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
    }

    // TotalAmount
    [Fact]
    public void Dado_TotalZero_Quando_Validar_Entao_RetornaErro()
    {
        // Arrange
        var command = CreateValidCommand() with { TotalAmount = 0 };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Dado_TotalNegativo_Quando_Validar_Entao_RetornaErro()
    {
        // Arrange
        var command = CreateValidCommand() with { TotalAmount = -1 };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
    }

    // Street
    [Fact]
    public void Dado_RuaVazia_Quando_Validar_Entao_RetornaErro()
    {
        // Arrange
        var command = CreateValidCommand() with { Street = "" };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == "Street");
    }

    [Fact]
    public void Dado_RuaMuitoGrande_Quando_Validar_Entao_RetornaErro()
    {
        // Arrange
        var command = CreateValidCommand() with { Street = new string('a', 151) };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
    }

    // City
    [Fact]
    public void Dado_CidadeVazia_Quando_Validar_Entao_RetornaErro()
    {
        // Arrange
        var command = CreateValidCommand() with { City = "" };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == "City");
    }

    [Fact]
    public void Dado_CidadeMuitoGrande_Quando_Validar_Entao_RetornaErro()
    {
        // Arrange
        var command = CreateValidCommand() with { City = new string('a', 101) };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
    }

    // State
    [Fact]
    public void Dado_EstadoVazio_Quando_Validar_Entao_RetornaErro()
    {
        // Arrange
        var command = CreateValidCommand() with { State = "" };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Dado_EstadoComMaisDeDoisCaracteres_Quando_Validar_Entao_RetornaErro()
    {
        // Arrange
        var command = CreateValidCommand() with { State = "SPT" };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
    }

    // Cep
    [Fact]
    public void Dado_CepVazio_Quando_Validar_Entao_RetornaErro()
    {
        // Arrange
        var command = CreateValidCommand() with { Cep = "" };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Dado_CepInvalido_Quando_Validar_Entao_RetornaMensagemCepInvalido()
    {
        // Arrange
        var command = CreateValidCommand() with { Cep = "123" };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.ErrorMessage == "CEP inválido.");
    }

    private static CreateOrderCommand CreateValidCommand()
        => new(
            Customer: "John Doe",
            TotalAmount: 100,
            Street: "Street 1",
            City: "City",
            State: "SP",
            Cep: "88550000");
}
