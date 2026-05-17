using Application.Common.Markers;
using Domain.OrdensServico.Gateways;

namespace Application.OrdensServico.UseCases.RemoverProduto;

public class RemoverProdutoUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IRemoverProdutoOrdemServicoOutputPort _outputPort;

    public RemoverProdutoUseCase(IOrdemServicoGateway gateway, IRemoverProdutoOrdemServicoOutputPort outputPort)
    {
        _gateway    = gateway;
        _outputPort = outputPort;
    }

    public async Task Execute(RemoverProdutoInput input, CancellationToken ct = default)
    {
        var ordemServico = await _gateway.BuscarComProdutos(input.IdOrdemServico, ct);
        if (ordemServico is null) { _outputPort.NaoEncontrado(); return; }

        ordemServico.RemoverProduto(input.IdProduto);
        await _gateway.Atualizar(ordemServico, ct);
        _outputPort.Ok();
    }
}
