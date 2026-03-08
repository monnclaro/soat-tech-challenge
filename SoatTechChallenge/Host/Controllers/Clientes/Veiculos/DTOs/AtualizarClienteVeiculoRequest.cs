namespace SoatTechChallenge.Host.Controllers.Clientes.Veiculos.DTOs;

public record AtualizarClienteVeiculoRequest(string Placa, string Marca, string Modelo, int Ano);