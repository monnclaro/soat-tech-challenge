using SoatTechChallenge.Application.Clientes.DTOs;
using SoatTechChallenge.Domain.Clientes.Enums;

namespace SoatTechChallenge.Application.Clientes.Services.Validators;

public interface IClienteValidatorService
{
    Task<(TipoDocumentoCliente tipo, string documento)> Validar(InserirClienteRequest request);
}