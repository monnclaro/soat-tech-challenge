using System.ComponentModel.DataAnnotations;

namespace Application.Clientes.Veiculos.DTOs.Requests;

public record AtualizarVeiculoRequest(
    [Required]
    [MaxLength(8)]
    string Placa,

    [Required]
    [MaxLength(50)]
    string Marca,

    [Required]
    [MaxLength(80)]
    string Modelo,

    [Required]
    int Ano
);