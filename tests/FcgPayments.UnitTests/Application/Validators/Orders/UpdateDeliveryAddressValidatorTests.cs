using FcgPayments.Application.Commands.Orders;
using FcgPayments.Application.Validators.Orders;
using Shouldly;

namespace FcgPayments.UnitTests.Application.Validators.Orders;

public class UpdateDeliveryAddressValidatorTests
{
    private readonly UpdateDeliveryAddressValidator _validator;

    public UpdateDeliveryAddressValidatorTests()
    {
        _validator = new UpdateDeliveryAddressValidator();
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

    [Fact]
    public void Dado_IdVazio_Quando_Validar_Entao_RetornaErro()
    {
        // Arrange
        var command = CreateValidCommand() with { Id = Guid.Empty };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == "Id");
    }

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
    public void Dado_CepFormatoInvalido_Quando_Validar_Entao_RetornaErroComMensagem()
    {
        // Arrange
        var command = CreateValidCommand() with { Cep = "123" };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x =>
            x.ErrorMessage == "O CEP deve estar no formato 00000-000 ou apenas números.");
    }

    [Fact]
    public void Dado_CepFormatoValidoComHifen_Quando_Validar_Entao_Aceita()
    {
        // Arrange
        var command = CreateValidCommand() with { Cep = "88550-000" };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    private static UpdateDeliveryAddressCommand CreateValidCommand()
        => new()
        {
            Id = Guid.NewGuid(),
            Street = "Street 1",
            City = "City",
            State = "SP",
            Cep = "88550000",
            UserId = "user-1",
            IsAdmin = false
        };
}
