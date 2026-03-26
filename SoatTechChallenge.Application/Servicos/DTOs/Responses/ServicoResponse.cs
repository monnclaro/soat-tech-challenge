namespace SoatTechChallenge.Application.Servicos.DTOs.Responses;

public class ServicoResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public decimal Valor { get; set; }
    
    public ServicoResponse(Guid id, string nome, string descricao, decimal valor)
    {
        Id = id;
        Nome = nome;
        Descricao = descricao;
        Valor = valor;
    }
}