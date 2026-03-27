using MockQueryable.Moq;
using Moq;
using SoatTechChallenge.Application.Clientes.DTOs;
using SoatTechChallenge.Application.Clientes.Services.Validators;
using SoatTechChallenge.Domain.Clientes;
using SoatTechChallenge.Domain.Clientes.Enums;
using SoatTechChallenge.Domain.Common.Exceptions;
using SoatTechChallenge.Domain.Common.Interfaces;
using Xunit;

namespace SoatTechChallenge.Tests.Clientes.Unit;

public class ClienteValidatorServiceTests
{
    private readonly Mock<IRepository<Cliente>> _repoMock = new();
    private readonly ClienteValidatorService _sut;

    // CPFs e CNPJs matematicamente válidos para os testes
    private const string CpfValido = "529.982.247-25";
    private const string CpfValidoLimpo = "52998224725";
    private const string CnpjValido = "11.222.333/0001-81";
    private const string CnpjValidoLimpo = "11222333000181";
    private const string CpfTodosIguais = "11111111111";
    private const string CnpjTodosIguais = "11111111111111";

    public ClienteValidatorServiceTests()
    {
        _sut = new ClienteValidatorService(_repoMock.Object);
        SetupSemDuplicados(); // padrão: nenhum cliente cadastrado
    }

    // ────────────────────────────────────────────────────────────
    // NormalizarDocumento
    // ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]          // somente letras → sem dígitos
    [InlineData("!!!---")]
    public async Task Validar_QuandoDocumentoSemDigitos_LancaDomainException(string documento)
    {
        var request = new InserirClienteRequest("Nome", documento);
        await Assert.ThrowsAsync<DomainException>(() => _sut.Validar(request));
    }

    // ────────────────────────────────────────────────────────────
    // ValidarTamanhoDocumento
    // ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("1234567890")]      // 10 dígitos
    [InlineData("123456789012")]    // 12 dígitos
    [InlineData("123456789012345")] // 15 dígitos
    public async Task Validar_QuandoTamanhoInvalido_LancaDomainException(string documento)
    {
        var request = new InserirClienteRequest("Nome", documento);
        await Assert.ThrowsAsync<DomainException>(() => _sut.Validar(request));
    }

    // ────────────────────────────────────────────────────────────
    // IdentificarTipoDocumento
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validar_Com11Digitos_IdentificaComoCpf()
    {
        var request = new InserirClienteRequest("Nome", CpfValido);

        var (tipo, _) = await _sut.Validar(request);

        Assert.Equal(TipoDocumentoCliente.Cpf, tipo);
    }

    [Fact]
    public async Task Validar_Com14Digitos_IdentificaComoCnpj()
    {
        var request = new InserirClienteRequest("Nome", CnpjValido);

        var (tipo, _) = await _sut.Validar(request);

        Assert.Equal(TipoDocumentoCliente.Cnpj, tipo);
    }

    // ────────────────────────────────────────────────────────────
    // NormalizarDocumento — remove máscara
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validar_ComCpfMascarado_RetornaApenasDigitos()
    {
        var request = new InserirClienteRequest("Nome", CpfValido);

        var (_, documento) = await _sut.Validar(request);

        Assert.Equal(CpfValidoLimpo, documento);
    }

    [Fact]
    public async Task Validar_ComCnpjMascarado_RetornaApenasDigitos()
    {
        var request = new InserirClienteRequest("Nome", CnpjValido);

        var (_, documento) = await _sut.Validar(request);

        Assert.Equal(CnpjValidoLimpo, documento);
    }

    // ────────────────────────────────────────────────────────────
    // ValidarDocumentoDuplicado
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validar_QuandoCpfJaCadastrado_LancaConflictException()
    {
        var clienteExistente = CriarCliente(CpfValidoLimpo, TipoDocumentoCliente.Cpf);
        SetupComCliente(clienteExistente);

        var request = new InserirClienteRequest("Nome", CpfValido);

        await Assert.ThrowsAsync<ConflictException>(() => _sut.Validar(request));
    }

    [Fact]
    public async Task Validar_QuandoCnpjJaCadastrado_LancaConflictException()
    {
        var clienteExistente = CriarCliente(CnpjValidoLimpo, TipoDocumentoCliente.Cnpj);
        SetupComCliente(clienteExistente);

        var request = new InserirClienteRequest("Nome", CnpjValido);

        await Assert.ThrowsAsync<ConflictException>(() => _sut.Validar(request));
    }

    [Fact]
    public async Task Validar_MensagemConflict_ContemTipoEDocumento_Cpf()
    {
        SetupComCliente(CriarCliente(CpfValidoLimpo, TipoDocumentoCliente.Cpf));

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.Validar(new InserirClienteRequest("Nome", CpfValido)));

        Assert.Contains("CPF", ex.Message);
        Assert.Contains(CpfValidoLimpo, ex.Message);
    }

    [Fact]
    public async Task Validar_MensagemConflict_ContemTipoEDocumento_Cnpj()
    {
        SetupComCliente(CriarCliente(CnpjValidoLimpo, TipoDocumentoCliente.Cnpj));

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.Validar(new InserirClienteRequest("Nome", CnpjValido)));

        Assert.Contains("CNPJ", ex.Message);
        Assert.Contains(CnpjValidoLimpo, ex.Message);
    }

    // ────────────────────────────────────────────────────────────
    // ValidarDigitos — CPF
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validar_CpfTodosDigitosIguais_LancaDomainException()
    {
        var request = new InserirClienteRequest("Nome", CpfTodosIguais);
        await Assert.ThrowsAsync<DomainException>(() => _sut.Validar(request));
    }

    [Theory]
    [InlineData("12345678901")] // dígitos verificadores errados
    [InlineData("00000000091")] // sequência inválida
    public async Task Validar_CpfComDigitoVerificadorInvalido_LancaDomainException(string cpf)
    {
        var request = new InserirClienteRequest("Nome", cpf);
        await Assert.ThrowsAsync<DomainException>(() => _sut.Validar(request));
    }

    [Fact]
    public async Task Validar_CpfValido_NaoLancaExcecao()
    {
        var request = new InserirClienteRequest("Nome", CpfValido);
        var ex = await Record.ExceptionAsync(() => _sut.Validar(request));
        Assert.Null(ex);
    }

    // ────────────────────────────────────────────────────────────
    // ValidarDigitos — CNPJ
    // ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validar_CnpjTodosDigitosIguais_LancaDomainException()
    {
        var request = new InserirClienteRequest("Nome", CnpjTodosIguais);
        await Assert.ThrowsAsync<DomainException>(() => _sut.Validar(request));
    }

    [Theory]
    [InlineData("12345678000190")] // dígitos verificadores errados
    [InlineData("00000000000000")] // sequência inválida (todos zeros → todos iguais)
    public async Task Validar_CnpjComDigitoVerificadorInvalido_LancaDomainException(string cnpj)
    {
        var request = new InserirClienteRequest("Nome", cnpj);
        await Assert.ThrowsAsync<DomainException>(() => _sut.Validar(request));
    }

    [Fact]
    public async Task Validar_CnpjValido_NaoLancaExcecao()
    {
        var request = new InserirClienteRequest("Nome", CnpjValido);
        var ex = await Record.ExceptionAsync(() => _sut.Validar(request));
        Assert.Null(ex);
    }

    // ────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────

    private void SetupSemDuplicados()
    {
        _repoMock.Setup(r => r.GetQueryable())
            .Returns(Enumerable.Empty<Cliente>().AsQueryable().BuildMock());
    }

    private void SetupComCliente(Cliente cliente)
    {
        _repoMock.Setup(r => r.GetQueryable())
            .Returns(new[] { cliente }.AsQueryable().BuildMock());
    }

    private static Cliente CriarCliente(string documento, TipoDocumentoCliente tipo)
    {
        var c = new Cliente();
        c.Inserir("Cliente Existente", documento, tipo);
        return c;
    }
}