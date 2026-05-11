namespace Application.Produtos.DTOs.Responses;

public record ProdutoResponse(Guid Id, string Nome, string Descricao, decimal Valor, decimal QuantidadeEmEstoque);