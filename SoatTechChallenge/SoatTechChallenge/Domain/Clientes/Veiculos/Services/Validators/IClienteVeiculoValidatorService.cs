using SoatTechChallenge.Host.Controllers.Clientes.Veiculos.DTOs;

namespace SoatTechChallenge.Domain.Clientes.Veiculos.Services.Validators;

public interface IClienteVeiculoValidatorService
{
    Task Validar(Guid idCliente, InserirClienteVeiculoRequest request);
    Task Validar(AtualizarClienteVeiculoRequest request);
}