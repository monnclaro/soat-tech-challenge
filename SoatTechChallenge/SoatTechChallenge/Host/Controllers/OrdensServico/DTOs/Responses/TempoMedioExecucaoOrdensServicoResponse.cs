namespace SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Responses;

public class TempoMedioExecucaoOrdensServicoResponse
{
    public double TempoMedioMinutos { get; set; }
    public double TempoMinimoMinutos { get; set; }
    public double TempoMaximoMinutos { get; set; }
    public double TotalOrdens { get; set; }
}