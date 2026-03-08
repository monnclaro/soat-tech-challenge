namespace SoatTechChallenge.Host.Controllers.Produtos.DTOs;

public record InserirProdutoRequest(string Nome, string Descricao, decimal Valor, decimal QuantidadeEmEstoque);