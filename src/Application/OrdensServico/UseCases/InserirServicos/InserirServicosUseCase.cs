using Application.Common.Markers;
using Application.Servicos.Queries;
using Domain.OrdensServico.Gateways;
using Domain.OrdensServico.Servicos;
using Domain.Servicos.Gateways;

namespace Application.OrdensServico.UseCases.InserirServicos;

public class InserirServicosUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IServicoGateway _servicoGateway;
    private readonly IInserirServicosOutputPort _outputPort;

    public InserirServicosUseCase(IOrdemServicoGateway gateway, IServicoGateway servicoGateway, IInserirServicosOutputPort outputPort)
    {
        _gateway        = gateway;
        _servicoGateway = servicoGateway;
        _outputPort     = outputPort;
    }

    public async Task Execute(InserirServicosOrdemServicoInput servicoInput, CancellationToken ct = default)
    {
        var ordemServico = await _gateway.BuscarComServicosProdutos(servicoInput.IdOrdemServico, ct);
        if (ordemServico is null) { _outputPort.NaoEncontrado(); return; }

        var ids = servicoInput.Servicos.Select(s => s.IdServico).ToList();
        var dicionario = await _servicoGateway.BuscarPorIds(ids, ct);

        var servicosInserir = servicoInput.Servicos
            .Where(s => dicionario.ContainsKey(s.IdServico))
            .Select(s =>
            {
                var servico = dicionario[s.IdServico];
                return new OrdemServicoServico(servicoInput.IdOrdemServico, servico.Id, servico.Nome, servico.Valor);
            }).ToList();

        ordemServico.InserirServicos(servicosInserir);
        await _gateway.Atualizar(ordemServico, ct);
        _outputPort.Ok();
    }
}
