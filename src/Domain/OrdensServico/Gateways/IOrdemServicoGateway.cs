

namespace Domain.OrdensServico.Gateways;

public interface IOrdemServicoGateway
{
    Task<OrdemServico?> BuscarPorId(Guid id, CancellationToken ct = default);
    Task<OrdemServico?> BuscarComServicos(Guid id, CancellationToken ct = default);
    Task<OrdemServico?> BuscarComProdutos(Guid id, CancellationToken ct = default);
    Task<OrdemServico?> BuscarComServicosProdutos(Guid id, CancellationToken ct = default);
    Task Salvar(OrdemServico ordemServico, CancellationToken ct = default);
    Task Atualizar(OrdemServico ordemServico, CancellationToken ct = default);
    Task Remover(OrdemServico ordemServico, CancellationToken ct = default);
}
