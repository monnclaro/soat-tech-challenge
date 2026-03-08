namespace SoatTechChallenge.Host.Controllers.Produtos.DTOs;

public record AtualizarProdutoRequest(string Nome, string Descricao, decimal Valor, decimal QuantidadeEmEstoque);