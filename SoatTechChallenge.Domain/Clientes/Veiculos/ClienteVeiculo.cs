
namespace SoatTechChallenge.Domain.Clientes.Veiculos;

public class ClienteVeiculo
{
    public Guid Id { get; private set; }
    public Guid IdCliente { get; private set; }
    public string Placa { get; private set; }
    public string Marca { get; private set; }
    public string Modelo { get; private set; }
    public int Ano { get; private set; }
    public DateTime DataCriacao { get; private set; }

    public ClienteVeiculo() { }

    public void Inserir(Guid idCliente, string placa, string marca, string modelo, int ano)
    {
        Id = Guid.NewGuid();
        IdCliente = idCliente;
        Placa = placa.ToUpper();
        Marca = marca;
        Modelo = modelo;
        Ano = ano;
        DataCriacao = DateTime.UtcNow;
    }

    public void Atualizar(string placa, string marca, string modelo, int ano)
    {
        Placa = placa.ToUpper();
        Marca = marca;
        Modelo = modelo;
        Ano = ano;
    }
}