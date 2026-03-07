using SoatTechChallenge.Clientes.Controllers.DTOs;
using SoatTechChallenge.Clientes.Enums;

namespace SoatTechChallenge.Clientes.Services.Validators;

public interface IClienteValidatorService
{
    Task<(TipoDocumentoCliente tipo, string documento)> Validar(InserirClienteRequest request);
}