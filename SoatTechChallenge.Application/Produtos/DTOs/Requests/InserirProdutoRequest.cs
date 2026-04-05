using System.ComponentModel.DataAnnotations;

namespace SoatTechChallenge.Application.Produtos.DTOs.Requests;

public class InserirProdutoRequest
{
    [Required]
    [MaxLength(150)]
    public string Nome { get; set; }

    [Required]
    [MaxLength(500)]
    public string Descricao { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Valor { get; set; }

    [Range(0, double.MaxValue)]
    public decimal QuantidadeEmEstoque { get; set; }

    public InserirProdutoRequest(string nome, string descricao, decimal valor, decimal quantidadeEmEstoque)
    {
        Nome = nome;
        Descricao = descricao;
        Valor = valor;
        QuantidadeEmEstoque = quantidadeEmEstoque;
    }
}