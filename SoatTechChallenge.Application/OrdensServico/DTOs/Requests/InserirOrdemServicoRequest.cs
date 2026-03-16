namespace SoatTechChallenge.Application.OrdensServico.DTOs.Requests;

public class InserirOrdemServicoRequest
{
    public Guid IdCliente { get; set; }
    public Guid IdVeiculo { get; set; }
    public List<Guid> IdsServicos { get; init; } = new();
}