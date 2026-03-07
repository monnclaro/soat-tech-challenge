namespace SoatTechChallenge.Host.Controllers.Produtos.DTOs;

public record InserirProdutoRequest(string Nome, string Descricao, decimal Preco, decimal QuantidadeEmEstoque);