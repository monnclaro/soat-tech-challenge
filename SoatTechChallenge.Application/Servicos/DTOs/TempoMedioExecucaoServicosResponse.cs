namespace SoatTechChallenge.Application.Servicos.DTOs;

public class TempoMedioExecucaoServicosResponse
{
    public string Servico { get; set; }
    public double TempoMedioMinutos { get; set; }
    public double TempoMinimoMinutos { get; set; }
    public double TempoMaximoMinutos { get; set; }
}