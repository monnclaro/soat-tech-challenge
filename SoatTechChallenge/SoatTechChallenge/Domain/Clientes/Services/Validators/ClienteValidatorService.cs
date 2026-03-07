using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Domain.Clientes.Enums;
using SoatTechChallenge.Host.Controllers.Clientes.DTOs;
using SoatTechChallenge.Host.Middlewares.Exceptions;
using SoatTechChallenge.Infrastructure.Common;
using SoatTechChallenge.Infrastructure.Interfaces;
using SoatTechChallenge.Middlewares.Exceptions;

namespace SoatTechChallenge.Domain.Clientes.Services.Validators;

public class ClienteValidatorService : IClienteValidatorService, ITransientService
{
    private readonly IRepository<Cliente> _clienteRepository;

    public ClienteValidatorService(IRepository<Cliente> clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<(TipoDocumentoCliente tipo, string documento)> Validar(InserirClienteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            throw new ConflictException("O campo 'Nome' é obrigatório.");
        }

        var documento = LimparDocumento(request.Documento);
        if (string.IsNullOrWhiteSpace(documento))
        {
            throw new ConflictException("O campo 'Documento' é obrigatório.");
        }

        var tipoDocumento = IdentificarTipo(documento);
        var tipo = tipoDocumento == TipoDocumentoCliente.Cpf ? "CPF" : "CNPJ";

        if (await _clienteRepository.Query().AsNoTracking().AnyAsync(l => l.Documento == documento))
        {
            throw new ConflictException($"Já existe um cliente cadastrado com o {tipo} '{documento}'.");
        }

        var valido = tipoDocumento == TipoDocumentoCliente.Cpf
            ? ValidarCPF(documento)
            : ValidarCNPJ(documento);

        if (!valido)
        {
            throw new DomainException($"{tipo} inválido: '{documento}'.");
        }

        return (tipoDocumento, documento);
    }
    
    public TipoDocumentoCliente IdentificarTipo(string documento)
    {
        return documento.Length switch
        {
            11 => TipoDocumentoCliente.Cpf,
            14 => TipoDocumentoCliente.Cnpj,
            _  => throw new DomainException("Documento inválido.")
        };
    }

    private static bool ValidarCPF(string cpf)
    {
        var digitos = LimparDocumento(cpf);

        if (digitos.Length != 11) return false;
        if (digitos.Distinct().Count() == 1) return false;

        return VerificarDigitosCPF(digitos);
    }

    private static bool VerificarDigitosCPF(string digits)
    {
        var soma = 0;
        for (var i = 0; i < 9; i++)
            soma += int.Parse(digits[i].ToString()) * (10 - i);

        var resto = soma % 11;
        var d1 = resto < 2 ? 0 : 11 - resto;

        if (d1 != int.Parse(digits[9].ToString())) return false;
      
        soma = 0;
        for (var i = 0; i < 10; i++)
            soma += int.Parse(digits[i].ToString()) * (11 - i);

        resto = soma % 11;
        var d2 = resto < 2 ? 0 : 11 - resto;

        return d2 == int.Parse(digits[10].ToString());
    }

    private static bool ValidarCNPJ(string cnpj)
    {
        var digitos = LimparDocumento(cnpj);

        if (digitos.Length != 14) return false;
        if (digitos.Distinct().Count() == 1) return false;

        return VerificarDigitosCNPJ(digitos);
    }

    private static bool VerificarDigitosCNPJ(string digits)
    {
        int[] pesosPrimeiro = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] pesosSegundo = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        var soma = 0;
        for (var i = 0; i < 12; i++)
            soma += int.Parse(digits[i].ToString()) * pesosPrimeiro[i];

        var resto = soma % 11;
        var d1 = resto < 2 ? 0 : 11 - resto;

        if (d1 != int.Parse(digits[12].ToString())) return false;
   
        soma = 0;
        for (var i = 0; i < 13; i++)
            soma += int.Parse(digits[i].ToString()) * pesosSegundo[i];

        resto = soma % 11;
        var d2 = resto < 2 ? 0 : 11 - resto;

        return d2 == int.Parse(digits[13].ToString());
    }

    private static string LimparDocumento(string documento) => Regex.Replace(documento, @"\D", string.Empty);
}
