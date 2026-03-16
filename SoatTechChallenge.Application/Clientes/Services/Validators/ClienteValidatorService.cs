using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SoatTechChallenge.Application.Clientes.DTOs;
using SoatTechChallenge.Application.Common.Interfaces;
using SoatTechChallenge.Domain.Clientes;
using SoatTechChallenge.Domain.Clientes.Enums;
using SoatTechChallenge.Domain.Common.Exceptions;
using SoatTechChallenge.Domain.Common.Interfaces;

namespace SoatTechChallenge.Application.Clientes.Services.Validators;

public class ClienteValidatorService : IClienteValidatorService, IScopedService
{
    private readonly IRepository<Cliente> _clienteRepository;

    public ClienteValidatorService(IRepository<Cliente> clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<(TipoDocumentoCliente tipo, string documento)> Validar(InserirClienteRequest request)
    {
        var documento = NormalizarDocumento(request.Documento);
        ValidarTamanhoDocumento(documento);

        var tipoDocumento = IdentificarTipoDocumento(documento);
        await ValidarDocumentoDuplicado(documento, tipoDocumento);

        ValidarDigitosDocumento(documento, tipoDocumento);

        return (tipoDocumento, documento);
    }

    private static string NormalizarDocumento(string documento)
    {
        var digitos = Regex.Replace(documento, @"\D", string.Empty);

        if (string.IsNullOrWhiteSpace(digitos))
            throw new DomainException("O documento informado é inválido.");

        return digitos;
    }

    private static void ValidarTamanhoDocumento(string documento)
    {
        if (documento.Length != 11 && documento.Length != 14)
        {
            throw new DomainException("O documento deve conter 11 dígitos (CPF) ou 14 dígitos (CNPJ).");
        }
    }

    private static TipoDocumentoCliente IdentificarTipoDocumento(string documento)
    {
        return documento.Length == 11 ? TipoDocumentoCliente.Cpf : TipoDocumentoCliente.Cnpj;
    }

    private async Task ValidarDocumentoDuplicado(string documento, TipoDocumentoCliente tipo)
    {
        var tipoDescricao = tipo == TipoDocumentoCliente.Cpf ? "CPF" : "CNPJ";

        var clienteExistente = await _clienteRepository
            .GetQueryable()
            .AsNoTracking()
            .AnyAsync(l => l.Documento == documento);

        if (clienteExistente) throw new ConflictException($"Já existe um cliente cadastrado com o {tipoDescricao} '{documento}'.");
    }

    private static void ValidarDigitosDocumento(string documento, TipoDocumentoCliente tipo)
    {
        var documentoValido = tipo switch
        {
            TipoDocumentoCliente.Cpf => ValidarCpf(documento),
            TipoDocumentoCliente.Cnpj => ValidarCnpj(documento),
            _ => false
        };

        if (!documentoValido)
        {
            var tipoDescricao = tipo == TipoDocumentoCliente.Cpf ? "CPF" : "CNPJ";
            throw new DomainException($"{tipoDescricao} inválido: '{documento}'.");
        }
    }

    private static bool ValidarCpf(string cpf)
    {
        if (cpf.Length != 11)
            return false;

        if (cpf.Distinct().Count() == 1)
            return false;

        int soma = 0;

        for (int i = 0; i < 9; i++)
            soma += (cpf[i] - '0') * (10 - i);

        int resto = soma % 11;
        int digito1 = resto < 2 ? 0 : 11 - resto;

        if (digito1 != cpf[9] - '0')
            return false;

        soma = 0;

        for (int i = 0; i < 10; i++)
            soma += (cpf[i] - '0') * (11 - i);

        resto = soma % 11;
        int digito2 = resto < 2 ? 0 : 11 - resto;

        return digito2 == cpf[10] - '0';
    }

    private static bool ValidarCnpj(string cnpj)
    {
        if (cnpj.Length != 14)
            return false;

        if (cnpj.Distinct().Count() == 1)
            return false;

        int[] pesosPrimeiro = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] pesosSegundo = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        int soma = 0;

        for (int i = 0; i < 12; i++)
            soma += (cnpj[i] - '0') * pesosPrimeiro[i];

        int resto = soma % 11;
        int digito1 = resto < 2 ? 0 : 11 - resto;

        if (digito1 != cnpj[12] - '0')
            return false;

        soma = 0;

        for (int i = 0; i < 13; i++)
            soma += (cnpj[i] - '0') * pesosSegundo[i];

        resto = soma % 11;
        int digito2 = resto < 2 ? 0 : 11 - resto;

        return digito2 == cnpj[13] - '0';
    }
}
