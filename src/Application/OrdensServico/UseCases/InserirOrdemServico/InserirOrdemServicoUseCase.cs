using Application.Common.Markers;
using Application.Servicos.Queries;
using Domain.Clientes.Gateways;
using Domain.OrdensServico;
using Domain.OrdensServico.Gateways;
using Domain.OrdensServico.Produtos;
using Domain.OrdensServico.Servicos;
using Domain.Produtos.Gateways;
using Domain.Servicos.Gateways;

namespace Application.OrdensServico.UseCases.InserirOrdemServico;

public class InserirOrdemServicoUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IClienteGateway _clienteGateway;
    private readonly IServicoGateway _servicoGateway;
    private readonly IProdutoGateway _produtoGateway;
    private readonly IInserirOrdemServicoOutputPort _outputPort;

    public InserirOrdemServicoUseCase(
        IOrdemServicoGateway gateway,
        IClienteGateway clienteGateway,
        IServicoGateway servicoGateway,
        IProdutoGateway produtoGateway,
        IInserirOrdemServicoOutputPort outputPort)
    {
        _gateway        = gateway;
        _clienteGateway = clienteGateway;
        _servicoGateway = servicoGateway;
        _produtoGateway = produtoGateway;
        _outputPort     = outputPort;
    }

    public async Task Execute(InserirOrdemServicoInput input, CancellationToken ct = default)
    {
        var cliente = await _clienteGateway.BuscarComVeiculos(input.IdCliente, ct);
        if (cliente is null) { _outputPort.ClienteNaoEncontrado(); return; }

        if (cliente.Veiculos.All(v => v.Id != input.IdVeiculo))
        {
            _outputPort.VeiculoNaoPertenceAoCliente(cliente.Nome);
            return;
        }

        var ordemServico = new OrdemServico();

        var servicos = new List<OrdemServicoServico>();
        if (input.IdsServicos.Any())
        {
            var dicionarioServicos = await _servicoGateway.BuscarPorIds(input.IdsServicos, ct);
            servicos = input.IdsServicos
                .Where(id => dicionarioServicos.ContainsKey(id))
                .Select(id =>
                {
                    var s = dicionarioServicos[id];
                    return new OrdemServicoServico(ordemServico.Id, s.Id, s.Nome, s.Valor);
                }).ToList();
        }

        var produtos = new List<OrdemServicoProduto>();
        if (input.Produtos.Any())
        {
            var ids = input.Produtos.Select(p => p.IdProduto).Distinct().ToList();
            var dicionarioProdutos = await _produtoGateway.BuscarDicionarioPorIds(ids, ct);

            produtos = input.Produtos
                .Where(p => dicionarioProdutos.ContainsKey(p.IdProduto))
                .Select(p =>
                {
                    var produto = dicionarioProdutos[p.IdProduto];
                    return new OrdemServicoProduto(ordemServico.Id, produto.Id, produto.Nome, produto.Valor, p.Quantidade);
                }).ToList();
        }

        ordemServico.Inserir(input.IdCliente, input.IdVeiculo, servicos, produtos);

        await _gateway.Salvar(ordemServico, ct);
        _outputPort.Ok(ordemServico.Id);
    }
}
