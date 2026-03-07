namespace SoatTechChallenge.Clientes.Controllers.Veiculos.DTOs;

 public record ClienteVeiculoResponse(
 Guid Id,
 Guid IdCliente,
 string Placa,
 string Marca,
 string Modelo,
 int Ano,
 DateTime DataCriacao
);