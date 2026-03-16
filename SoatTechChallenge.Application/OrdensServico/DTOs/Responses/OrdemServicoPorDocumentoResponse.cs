namespace SoatTechChallenge.Application.OrdensServico.DTOs.Responses;

public class OrdemServicoPorDocumentoResponse
{
    public OrdemServicoClientePorDocumentoResponse Cliente { get; }
    public OrdemServicoVeiculoPorDocumentoResponse Veiculo { get; }
    public string Status { get; }

    public OrdemServicoPorDocumentoResponse(
        OrdemServicoClientePorDocumentoResponse cliente,
        OrdemServicoVeiculoPorDocumentoResponse veiculo,
        string status)
    {
        Cliente = cliente;
        Veiculo = veiculo;
        Status = status;
    }
}

public class OrdemServicoClientePorDocumentoResponse
{
    public string Nome { get; }
    public string Documento { get; }

    public OrdemServicoClientePorDocumentoResponse(string nome, string documento)
    {
        Nome = nome;
        Documento = documento;
    }
}

public class OrdemServicoVeiculoPorDocumentoResponse
{
    public string Placa { get; }
    public string Marca { get; }
    public string Modelo { get; }
    public int Ano { get; }

    public OrdemServicoVeiculoPorDocumentoResponse(string placa, string marca, string modelo, int ano)
    {
        Placa = placa;
        Marca = marca;
        Modelo = modelo;
        Ano = ano;
    }
}