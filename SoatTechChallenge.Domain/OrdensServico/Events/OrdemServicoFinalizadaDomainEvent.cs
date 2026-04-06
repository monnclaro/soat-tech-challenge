using SharedKernel;
using SoatTechChallenge.Domain.OrdensServico.Produtos;

namespace SoatTechChallenge.Domain.OrdensServico.Events;

public sealed record OrdemServicoFinalizadaDomainEvent(
    Guid IdOrdemServico,
    IReadOnlyList<OrdemServicoProduto> Produtos
) : IDomainEvent
{
    public DateTime OcurredAt { get; } = DateTime.UtcNow;
}