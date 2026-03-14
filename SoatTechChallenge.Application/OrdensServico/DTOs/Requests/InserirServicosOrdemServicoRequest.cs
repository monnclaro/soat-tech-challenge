namespace SoatTechChallenge.Application.OrdensServico.DTOs.Requests;

public class InserirServicosOrdemServicoRequest
{
    public List<InserirServicosOrdemServicoServicoRequest> Servicos { get; set; }

    public InserirServicosOrdemServicoRequest(List<InserirServicosOrdemServicoServicoRequest> servicos)
    {
        Servicos = servicos;
    }
}

public class InserirServicosOrdemServicoServicoRequest
{
    public Guid IdServico { get; set; }
    
    public InserirServicosOrdemServicoServicoRequest(Guid idServico)
    {
        IdServico = idServico;
    }
}