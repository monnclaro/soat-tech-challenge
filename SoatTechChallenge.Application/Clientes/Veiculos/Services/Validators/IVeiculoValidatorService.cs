using SoatTechChallenge.Application.Clientes.Veiculos.DTOs;
using SoatTechChallenge.Host.Controllers.Clientes.Veiculos.DTOs;

namespace SoatTechChallenge.Application.Clientes.Veiculos.Services.Validators;

public interface IVeiculoValidatorService
{
    Task Validar(Guid idCliente, InserirClienteVeiculoRequest request);
    Task Validar(Guid idVeiculo, AtualizarClienteVeiculoRequest request);
}