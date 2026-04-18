namespace SoatTechChallenge.Application.OrdensServico.DTOs.Responses;

public class OrdemServicoPorDocumentoResponse
{
    public string Status { get; }
    
    public OrdemServicoClientePorDocumentoResponse Cliente { get; }
    public OrdemServicoVeiculoPorDocumentoResponse Veiculo { get; }
    public List<OrdemServicoServicoPorDocumentoResponse> Servicos { get; }


    public OrdemServicoPorDocumentoResponse(
        string status,
        OrdemServicoClientePorDocumentoResponse cliente,
        OrdemServicoVeiculoPorDocumentoResponse veiculo,
        List<OrdemServicoServicoPorDocumentoResponse> servicos
    )
    {
        Cliente = cliente;
        Veiculo = veiculo;
        Status = status;
        Servicos = servicos;
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

public class OrdemServicoServicoPorDocumentoResponse
{
    public string Nome { get; }
    public string Status { get; }
    
    public OrdemServicoServicoPorDocumentoResponse(string nome, string status)
    {
        Nome = nome;
        Status = status;
    }
}