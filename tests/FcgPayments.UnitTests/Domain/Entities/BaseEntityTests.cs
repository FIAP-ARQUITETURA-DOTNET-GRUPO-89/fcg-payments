using FcgPayments.Domain.Entities;
using Shouldly;

namespace FcgPayments.UnitTests.Domain.Entities;

public class BaseEntityTests
{
    [Fact]
    public void Dado_NovaEntidade_Quando_Criada_Entao_PossuiIdECreatedAt()
    {
        // Arrange & Act
        var entity = CreateTestEntity();

        // Assert
        entity.Id.ShouldNotBe(Guid.Empty);
        entity.CreatedAt.ShouldBeGreaterThan(DateTime.MinValue);
        entity.UpdatedAt.ShouldBeNull();
    }

    [Fact]
    public void Dado_DuasEntidadesComMesmoId_Quando_Comparadas_Entao_SaoIguais()
    {
        // Arrange
        var entity1 = CreateTestEntity();
        var entity2 = CreateTestEntity();

        typeof(BaseEntity)
            .GetProperty("Id")!
            .SetValue(entity2, entity1.Id);

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Dado_EntidadesDiferentes_Quando_Comparadas_Entao_NaoSaoIguais()
    {
        // Arrange
        var entity1 = CreateTestEntity();
        var entity2 = CreateTestEntity();

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        result.ShouldBeFalse();
    }

    private static TestEntity CreateTestEntity() => new();

    private class TestEntity : BaseEntity { }
}
