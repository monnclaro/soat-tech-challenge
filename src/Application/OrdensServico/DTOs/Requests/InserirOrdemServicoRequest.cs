namespace Application.OrdensServico.DTOs.Requests;

public class InserirOrdemServicoRequest
{
    public Guid IdCliente { get; set; }
    public Guid IdVeiculo { get; set; }
    public List<Guid> IdsServicos { get; init; }

    public InserirOrdemServicoRequest(Guid idCliente ,Guid idVeiculo , List<Guid> idsServicos )
    {
        IdCliente = idCliente;
        IdVeiculo = idVeiculo;
        IdsServicos = idsServicos;
    }
}