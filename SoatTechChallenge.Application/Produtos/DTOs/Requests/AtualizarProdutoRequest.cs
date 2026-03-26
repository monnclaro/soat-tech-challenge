namespace SoatTechChallenge.Application.Produtos.DTOs.Requests;

public class AtualizarProdutoRequest
{
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public decimal Valor { get; set; }

    public AtualizarProdutoRequest() { }

    public AtualizarProdutoRequest(string nome, string descricao, decimal valor)
    {
        Nome = nome;
        Descricao = descricao;
        Valor = valor;
    }
}