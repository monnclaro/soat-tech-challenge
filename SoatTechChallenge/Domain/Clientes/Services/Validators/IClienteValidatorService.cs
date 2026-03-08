using SoatTechChallenge.Domain.Clientes.Enums;
using SoatTechChallenge.Host.Controllers.Clientes.DTOs;

namespace SoatTechChallenge.Domain.Clientes.Services.Validators;

public interface IClienteValidatorService
{
    Task<(TipoDocumentoCliente tipo, string documento)> Validar(InserirClienteRequest request);
}