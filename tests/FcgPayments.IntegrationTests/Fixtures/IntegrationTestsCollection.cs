namespace FcgPayments.IntegrationTests.Fixtures;

/// <summary>
/// Define uma coleção de testes de integração que compartilham a mesma instância de <see cref="IntegrationTestFixture"/>, evitando a recriação da infraestrutura (App, banco, etc.) a cada classe de teste.
/// </summary>
[CollectionDefinition("IntegrationTests")]
public class IntegrationTestsCollection : ICollectionFixture<IntegrationTestFixture>
{
}
