namespace SoatTechChallenge.Host.Controllers.Clientes.Veiculos.DTOs;

public record InserirClienteVeiculoRequest(string Placa, string Marca, string Modelo, int Ano);