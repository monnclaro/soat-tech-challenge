namespace SoatTechChallenge.Application.Servicos.DTOs.Responses;

public class TempoMedioExecucaoServicosResponse
{
    public string Servico { get; set; }
    public double TempoMinimoMinutos { get; set; }
    public double TempoMedioMinutos { get; set; }
    public double TempoMaximoMinutos { get; set; }

    public TempoMedioExecucaoServicosResponse() { }
    
    public TempoMedioExecucaoServicosResponse(string servico, double tempoMinimoMinutos, double tempoMedioMinutos, double tempoMaximoMinutos)
    {
        Servico = servico;
        TempoMinimoMinutos = tempoMinimoMinutos;
        TempoMedioMinutos = tempoMedioMinutos;
        TempoMaximoMinutos = tempoMaximoMinutos;
    }
    
}