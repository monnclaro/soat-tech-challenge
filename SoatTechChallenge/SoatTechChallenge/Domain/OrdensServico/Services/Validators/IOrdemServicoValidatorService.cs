using SoatTechChallenge.Host.Controllers.OrdensServico.DTOs.Requests;

namespace SoatTechChallenge.Domain.OrdensServico.Services.Validators;

public interface IOrdemServicoValidatorService
{
    Task Validar(InserirOrdemServicoRequest request);
}