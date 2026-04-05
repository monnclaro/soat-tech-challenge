using System.ComponentModel.DataAnnotations;

namespace SoatTechChallenge.Application.Clientes.DTOs.Requests;

public record InserirClienteRequest( 
    [Required]
    [MaxLength(100)]
    string Nome,
    string Documento);