using Domain.OrdensServico.Events;

namespace Application.Produtos.DTOs.Commands;

public class DecrementarQuantidadeEstoqueProdutosCommand
{
    public List<DecrementarQuantidadeEstoqueProdutosProdutoCommand> Produtos { get; set; } = new();

    public DecrementarQuantidadeEstoqueProdutosCommand()
    {
        
    }
    
    public DecrementarQuantidadeEstoqueProdutosCommand(OrdemServicoFinalizadaDomainEvent evento)
    {
        Produtos = evento.Produtos.Select(l => new DecrementarQuantidadeEstoqueProdutosProdutoCommand()
        {
            Id = l.IdProduto,
            Quantidade = l.Quantidade
        }).ToList();
    }
}

public class DecrementarQuantidadeEstoqueProdutosProdutoCommand
{
    public Guid Id { get; set; }
    public decimal Quantidade { get; set; }

    public DecrementarQuantidadeEstoqueProdutosProdutoCommand()
    {
        
    }
}