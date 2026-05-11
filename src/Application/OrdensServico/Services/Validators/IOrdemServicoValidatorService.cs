using Application.OrdensServico.DTOs.Requests;

namespace Application.OrdensServico.Services.Validators;

public interface IOrdemServicoValidatorService
{
    Task Validar(InserirOrdemServicoRequest request);
}