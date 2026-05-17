namespace Application.Servicos.DTOs;

public record TempoMedioExecucaoOutput(
    string Servico,
    double TempoMedioMinutos,
    double TempoMinimoMinutos,
    double TempoMaximoMinutos);
