using SoatTechChallenge.Application.Clientes.Veiculos.DTOs;
using SoatTechChallenge.Application.Clientes.Veiculos.DTOs.Requests;

namespace SoatTechChallenge.Application.Clientes.Veiculos.Services.Validators;

public interface IVeiculoValidatorService
{
    Task Validar(Guid idCliente, InserirVeiculoRequest request);
    Task Validar(Guid idVeiculo, AtualizarVeiculoRequest request);
}