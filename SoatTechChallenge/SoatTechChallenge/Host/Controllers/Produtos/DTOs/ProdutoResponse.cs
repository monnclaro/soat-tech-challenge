namespace SoatTechChallenge.Host.Controllers.Produtos.DTOs;

public record ProdutoResponse(Guid Id, string Nome, string Descricao, decimal Preco, decimal QuantidadeEmEstoque);