namespace Application.Clientes.Veiculos.DTOs.Responses;

 public record VeiculoResponse(
 Guid Id,
 Guid IdCliente,
 string Placa,
 string Marca,
 string Modelo,
 int Ano,
 DateTime DataCriacao
);