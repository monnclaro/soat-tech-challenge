using Domain.Clientes.Enums;
using Domain.Clientes.Veiculos;
using Domain.Common.Exceptions;
using SharedKernel;

namespace Domain.Clientes;

public class Cliente : Entity
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Documento { get; private set; }
    public TipoDocumentoCliente TipoDocumento { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public List<Veiculo> Veiculos { get; init; } = new();

    public Cliente() { }

    public void Inserir(string nome, string documento, TipoDocumentoCliente tipoDocumento)
    { 
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome é obrigatório.");
        
        if (string.IsNullOrWhiteSpace(documento))
            throw new DomainException("O documento é obrigatório.");
        
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

