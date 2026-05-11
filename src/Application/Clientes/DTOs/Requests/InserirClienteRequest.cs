using System.ComponentModel.DataAnnotations;

namespace Application.Clientes.DTOs.Requests;

public record InserirClienteRequest( 
    [Required]
    [MaxLength(100)]
    string Nome,
    string Documento);