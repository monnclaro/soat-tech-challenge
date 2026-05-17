using Application.Common.Markers;
using Domain.OrdensServico.Gateways;
using Domain.OrdensServico.Produtos;
using Domain.Produtos.Gateways;

namespace Application.OrdensServico.UseCases.InserirProdutos;

public class InserirProdutosUseCase : IUseCase
{
    private readonly IOrdemServicoGateway _gateway;
    private readonly IProdutoGateway _produtoGateway;
    private readonly IInserirProdutosOutputPort _outputPort;

    public InserirProdutosUseCase(IOrdemServicoGateway gateway, IProdutoGateway produtoGateway, IInserirProdutosOutputPort outputPort)
    {
        _gateway        = gateway;
        _produtoGateway = produtoGateway;
        _outputPort     = outputPort;
    }

    public async Task Execute(InserirProdutosInput input, CancellationToken ct = default)
    {
        var ordemServico = await _gateway.BuscarComServicosProdutos(input.IdOrdemServico, ct);
        if (ordemServico is null) { _outputPort.NaoEncontrado(); return; }

        var ids = input.Produtos.Select(p => p.IdProduto).Distinct().ToList();
        var dicionario = await _produtoGateway.BuscarDicionarioPorIds(ids, ct);

        var produtosInserir = new List<OrdemServicoProduto>();
        foreach (var item in input.Produtos.Where(p => dicionario.ContainsKey(p.IdProduto)))
        {
            var produto = dicionario[item.IdProduto];
            if (produto.QuantidadeEmEstoque < item.Quantidade)
            {
                _outputPort.EstoqueInsuficiente(
                    $"Estoque insuficiente para '{produto.Nome}'. Disponível: {produto.QuantidadeEmEstoque}, solicitado: {item.Quantidade}.");
                return;
            }
            produtosInserir.Add(new OrdemServicoProduto(input.IdOrdemServico, produto.Id, produto.Nome, produto.Valor, item.Quantidade));
        }

        ordemServico.InserirProdutos(produtosInserir);
        await _gateway.Atualizar(ordemServico, ct);
        _outputPort.Ok();
    }
}
