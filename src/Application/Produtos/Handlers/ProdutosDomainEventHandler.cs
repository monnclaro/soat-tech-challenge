using Application.Produtos.DTOs.Commands;
using Application.Produtos.Services;
using Domain.OrdensServico.Events;
using SharedKernel;

namespace Application.Produtos.Handlers;

internal sealed class ProdutosDomainEventHandler : IDomainEventHandler<OrdemServicoFinalizadaDomainEvent>
{
    private readonly IProdutoService _produtoService;

    public ProdutosDomainEventHandler(IProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    public async Task Handle(OrdemServicoFinalizadaDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await _produtoService.DecrementarEstoque(new DecrementarQuantidadeEstoqueProdutosCommand(domainEvent));
    }
}
