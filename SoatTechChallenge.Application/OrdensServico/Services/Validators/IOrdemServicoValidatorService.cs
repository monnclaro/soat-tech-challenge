using SoatTechChallenge.Application.OrdensServico.DTOs.Requests;

namespace SoatTechChallenge.Application.OrdensServico.Services.Validators;

public interface IOrdemServicoValidatorService
{
    Task Validar(InserirOrdemServicoRequest request);
}