using SharedKernel;
using SoatTechChallenge.Application.Produtos.DTOs.Commands;
using SoatTechChallenge.Application.Produtos.Services;
using SoatTechChallenge.Domain.OrdensServico.Events;

namespace SoatTechChallenge.Application.Produtos.Handlers;

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
