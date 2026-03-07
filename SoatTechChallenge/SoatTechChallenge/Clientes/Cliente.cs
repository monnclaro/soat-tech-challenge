using SoatTechChallenge.Clientes.Controllers.DTOs;
using SoatTechChallenge.Clientes.Enums;
using SoatTechChallenge.Clientes.Services.Validators;
using SoatTechChallenge.Clientes.Veiculos;

namespace SoatTechChallenge.Clientes;

public class Cliente
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string Documento { get; private set; }
    public TipoDocumentoCliente TipoDocumento { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public List<ClienteVeiculo> Veiculos { get; init; } = new();

    public Cliente() { }

    public async Task Inserir(InserirClienteRequest request, IClienteValidatorService validator)
    {
        var (tipoDocumento, documento) = await validator.Validar(request);
             
        Id = Guid.NewGuid();
        Nome = request.Nome;
        Documento = documento;
        TipoDocumento = tipoDocumento;
        DataCriacao = DateTime.UtcNow;
    }

    public void Atualizar(string nome)
    {
        Nome = nome;
    }
}

