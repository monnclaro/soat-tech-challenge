namespace Application.OrdensServico.DTOs.Responses;

public class OrdemServicoStatusResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = default!;
}