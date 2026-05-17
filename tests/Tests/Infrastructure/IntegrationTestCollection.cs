using Xunit;

namespace Tests.Infrastructure;
 
/// <summary>
/// Garante que os testes de integração NÃO rodem em paralelo entre si,
/// evitando conflito de containers ou banco de dados compartilhado.
/// Cada classe de teste de integração herda de IClassFixture<> ou usa [Collection].
/// </summary>
[CollectionDefinition(nameof(IntegrationTestCollection), DisableParallelization = true)]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestCollection>
{
}