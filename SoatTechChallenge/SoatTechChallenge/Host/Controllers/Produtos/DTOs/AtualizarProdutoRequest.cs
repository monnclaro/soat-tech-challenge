namespace SoatTechChallenge.Host.Controllers.Produtos.DTOs;

public record AtualizarProdutoRequest(string Nome, string Descricao, decimal Preco, decimal QuantidadeEmEstoque);