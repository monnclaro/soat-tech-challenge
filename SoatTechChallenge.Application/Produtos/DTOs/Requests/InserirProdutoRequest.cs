namespace SoatTechChallenge.Application.Produtos.DTOs.Requests;

public class InserirProdutoRequest
{
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public decimal Valor { get; set; }
    public decimal QuantidadeEmEstoque { get; set; }

    public InserirProdutoRequest()
    {
        
    }

    public InserirProdutoRequest(string nome, string descricao, decimal valor, decimal quantidadeEmEstoque)
    {
        Nome = nome;
        Descricao = descricao;
        Valor = valor;
        QuantidadeEmEstoque = quantidadeEmEstoque;
    }
}