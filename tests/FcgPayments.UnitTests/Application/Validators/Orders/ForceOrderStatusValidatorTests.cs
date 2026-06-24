using FcgPayments.Application.Commands.Orders;
using FcgPayments.Application.Validators.Orders;
using FcgPayments.Domain.Enums;
using Shouldly;

namespace FcgPayments.UnitTests.Application.Validators.Orders;

public class ForceOrderStatusValidatorTests
{
    private readonly ForceOrderStatusValidator _validator;

    public ForceOrderStatusValidatorTests()
    {
        _validator = new ForceOrderStatusValidator();
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
    public void Dado_StatusVazio_Quando_Validar_Entao_RetornaErro()
    {
        // Arrange
        var command = CreateValidCommand() with { NewStatus = "" };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == "NewStatus");
    }

    [Fact]
    public void Dado_StatusInvalido_Quando_Validar_Entao_RetornaErroComMensagem()
    {
        // Arrange
        var command = CreateValidCommand() with { NewStatus = "InvalidStatus" };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x =>
            x.ErrorMessage.Contains("Status inválido"));
    }

    [Fact]
    public void Dado_StatusValido_Quando_Validar_Entao_AceitaApproved()
    {
        // Arrange
        var command = CreateValidCommand() with { NewStatus = OrderStatus.Approved.ToString() };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Dado_StatusValido_Quando_Validar_Entao_AceitaRejected()
    {
        // Arrange
        var command = CreateValidCommand() with { NewStatus = OrderStatus.Rejected.ToString() };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    private static ForceOrderStatusCommand CreateValidCommand()
        => new()
        {
            Id = Guid.NewGuid(),
            NewStatus = OrderStatus.PendingPayment.ToString()
        };
}
