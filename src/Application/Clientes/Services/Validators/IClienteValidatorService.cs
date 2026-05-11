using Application.Clientes.DTOs.Requests;
using Domain.Clientes.Enums;

namespace Application.Clientes.Services.Validators;

public interface IClienteValidatorService
{
    Task<(TipoDocumentoCliente tipo, string documento)> Validar(InserirClienteRequest request);
}