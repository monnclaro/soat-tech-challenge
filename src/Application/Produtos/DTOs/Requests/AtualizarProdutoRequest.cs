using System.ComponentModel.DataAnnotations;

namespace Application.Produtos.DTOs.Requests;

public class AtualizarProdutoRequest
{
    [Required]
    [MaxLength(150)]
    public string Nome { get; set; }

    [Required]
    [MaxLength(500)]
    public string Descricao { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Valor { get; set; }

    public AtualizarProdutoRequest(string nome, string descricao, decimal valor)
    {
        Nome = nome;
        Descricao = descricao;
        Valor = valor;
    }
}