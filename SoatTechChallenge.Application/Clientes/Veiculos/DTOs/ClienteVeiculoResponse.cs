namespace SoatTechChallenge.Host.Controllers.Clientes.Veiculos.DTOs;

 public record ClienteVeiculoResponse(
 Guid Id,
 Guid IdCliente,
 string Placa,
 string Marca,
 string Modelo,
 int Ano,
 DateTime DataCriacao
);