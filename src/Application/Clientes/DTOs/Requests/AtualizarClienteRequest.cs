using System.ComponentModel.DataAnnotations;

namespace Application.Clientes.DTOs.Requests;

public record AtualizarClienteRequest(  
    [Required]
    [MaxLength(100)]
    string Nome);