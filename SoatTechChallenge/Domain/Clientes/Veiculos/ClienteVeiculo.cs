using SoatTechChallenge.Domain.Clientes.Veiculos.Services.Validators;
using SoatTechChallenge.Host.Controllers.Clientes.Veiculos.DTOs;

namespace SoatTechChallenge.Domain.Clientes.Veiculos;

public class ClienteVeiculo
{
    public Guid Id { get; private set; }
    public Guid IdCliente { get; private set; }
    public string Placa { get; private set; }
    public string Marca { get; private set; }
    public string Modelo { get; private set; }
    public int Ano { get; private set; }
    public DateTime DataCriacao { get; private set; }

    public ClienteVeiculo() { }

    public async Task Inserir(Guid idCliente, InserirClienteVeiculoRequest request, IClienteVeiculoValidatorService validatorService)
    {
        await validatorService.Validar(idCliente, request);
        
        Id = Guid.NewGuid();
        IdCliente = idCliente;
        Placa = request.Placa.ToUpper();
        Marca = request.Marca;
        Modelo = request.Modelo;
        Ano = request.Ano;
        DataCriacao = DateTime.UtcNow;
    }

    public async Task Atualizar(AtualizarClienteVeiculoRequest request, IClienteVeiculoValidatorService validatorService)
    {
        await validatorService.Validar(request);

        Placa = request.Placa.ToUpper();
        Marca = request.Marca;
        Modelo = request.Modelo;
        Ano = request.Ano;
    }
}