
using SoatTechChallenge.Clientes.Controllers.Veiculos.DTOs;

namespace SoatTechChallenge.Clientes.Veiculos.Services.Validators;

public interface IClienteVeiculoValidatorService
{
    Task Validar(Guid idCliente, InserirClienteVeiculoRequest request);
    Task Validar(AtualizarClienteVeiculoRequest request);
}