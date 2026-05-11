using System.ComponentModel.DataAnnotations;

namespace Application.Produtos.DTOs.Requests;

public class AtualizarQuantidadeEstoqueProdutoRequest
{
    [Range(0, double.MaxValue)]
    public decimal Quantidade { get; set; }

    public AtualizarQuantidadeEstoqueProdutoRequest(decimal quantidade)
    {
        Quantidade = quantidade;
    }
}