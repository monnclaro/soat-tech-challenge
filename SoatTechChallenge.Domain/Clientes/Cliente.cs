using SoatTechChallenge.Domain.Clientes.Enums;
using SoatTechChallenge.Domain.Clientes.Veiculos;


namespace SoatTechChallenge.Domain.Clientes;

public class Cliente
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Documento { get; private set; }
    public TipoDocumentoCliente TipoDocumento { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public List<ClienteVeiculo> Veiculos { get; init; } = new();

    public Cliente() { }

    public void Inserir(string nome, string documento, TipoDocumentoCliente tipoDocumento)
    {
        Id = Guid.NewGuid();
        Nome = nome;
        Documento = documento;
        TipoDocumento = tipoDocumento;
        DataCriacao = DateTime.UtcNow;
    }

    public void Atualizar(string nome)
    {
        Nome = nome;
    }
}

