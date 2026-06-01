namespace Application.Clientes.UseCases;

public record ClienteOutput(Guid Id, string Nome, string Documento, DateTime DataCriacao);
