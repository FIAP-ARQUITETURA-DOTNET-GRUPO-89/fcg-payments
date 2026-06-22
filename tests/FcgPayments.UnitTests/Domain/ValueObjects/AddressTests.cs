using FcgPayments.Domain.ValueObjects;
using FcgPayments.SharedKernel.Exceptions;
using Shouldly;

namespace FcgPayments.UnitTests.Domain.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Dado_EnderecoValido_Quando_Criar_Entao_CriaNormalizado()
    {
        // Arrange
        var street = "Street 1";
        var city = "City";
        var state = "sp";
        var cep = "88550-000";

        // Act
        var address = new Address(street, city, state, cep);

        // Assert
        address.Street.ShouldBe("Street 1");
        address.City.ShouldBe("City");
        address.State.ShouldBe("SP");
        address.Cep.ShouldBe("88550000");
    }

    // Street
    [Fact]
    public void Dado_RuaVazia_Quando_CriarEndereco_Entao_LancaExcecao()
    {
        // Arrange
        Action act = () => new Address("", "City", "SP", "88550000");

        // Act & Assert
        Should.Throw<InvalidAddressException>(act);
    }

    [Fact]
    public void Dado_RuaEmBranco_Quando_CriarEndereco_Entao_LancaExcecao()
    {
        // Arrange
        Action act = () => new Address("   ", "City", "SP", "88550000");

        // Act & Assert
        Should.Throw<InvalidAddressException>(act);
    }

    // City
    [Fact]
    public void Dado_CidadeVazia_Quando_CriarEndereco_Entao_LancaExcecao()
    {
        // Arrange
        Action act = () => new Address("Street", "", "SP", "88550000");

        // Act & Assert
        Should.Throw<InvalidAddressException>(act);
    }

    [Fact]
    public void Dado_CidadeEmBranco_Quando_CriarEndereco_Entao_LancaExcecao()
    {
        // Arrange
        Action act = () => new Address("Street", "   ", "SP", "88550000");

        // Act & Assert
        Should.Throw<InvalidAddressException>(act);
    }

    // State
    [Fact]
    public void Dado_EstadoVazio_Quando_CriarEndereco_Entao_LancaExcecao()
    {
        // Arrange
        Action act = () => new Address("Street", "City", "", "88550000");

        // Act & Assert
        Should.Throw<InvalidAddressException>(act);
    }

    [Fact]
    public void Dado_EstadoComTamanhoInvalido_Quando_CriarEndereco_Entao_LancaExcecao()
    {
        // Arrange
        Action act = () => new Address("Street", "City", "SPT", "88550000");

        // Act & Assert
        Should.Throw<InvalidAddressException>(act);
    }

    // Cep
    [Fact]
    public void Dado_CepVazio_Quando_CriarEndereco_Entao_LancaExcecao()
    {
        // Arrange
        Action act = () => new Address("Street", "City", "SP", "");

        // Act & Assert
        Should.Throw<InvalidAddressException>(act);
    }

    [Fact]
    public void Dado_CepComFormatoInvalido_Quando_CriarEndereco_Entao_LancaExcecao()
    {
        // Arrange
        Action act = () => new Address("Street", "City", "SP", "123");

        // Act & Assert
        Should.Throw<InvalidAddressException>(act);
    }

    [Fact]
    public void Dado_CepComCaracteresEspeciais_Quando_CriarEndereco_Entao_NormalizaCorretamente()
    {
        // Arrange
        var cep = "88.550-000";

        // Act
        var address = new Address("Street", "City", "SP", cep);

        // Assert
        address.Cep.ShouldBe("88550000");
    }

    [Fact]
    public void Dado_EstadoMinusculo_Quando_CriarEndereco_Entao_ConverteParaMaiusculo()
    {
        // Arrange
        var state = "sp";

        // Act
        var address = new Address("Street", "City", state, "88550000");

        // Assert
        address.State.ShouldBe("SP");
    }
}
