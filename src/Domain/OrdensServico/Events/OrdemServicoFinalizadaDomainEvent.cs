using Domain.OrdensServico.Produtos;
using SharedKernel;

namespace Domain.OrdensServico.Events;

public sealed record OrdemServicoFinalizadaDomainEvent(
    Guid IdOrdemServico,
    IReadOnlyList<OrdemServicoProduto> Produtos
) : IDomainEvent
{
    public DateTime OcurredAt { get; } = DateTime.UtcNow;
}