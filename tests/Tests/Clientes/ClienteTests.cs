using Domain.Clientes;
using Domain.Clientes.Enums;
using Domain.Clientes.ValueObjects;
using Domain.Common.Exceptions;

namespace Tests.Clientes;

public class ClienteTests
{
    // ── Cliente.Inserir ──────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Inserir_QuandoNomeInvalido_LancaDomainException(string? nome)
    {
        var cliente   = new Cliente();
        var documento = DocumentoCliente.Criar("12345678909");

        Assert.Throws<DomainException>(() => cliente.Inserir(nome!, documento));
    }

    [Fact]
    public void Inserir_QuandoDadosValidos_PopulaPropriedadesCorretamente()
    {
        var antes     = DateTime.UtcNow;
        var cliente   = new Cliente();
        var documento = DocumentoCliente.Criar("12345678909");

        cliente.Inserir("João Silva", documento);

        Assert.NotEqual(Guid.Empty, cliente.Id);
        Assert.Equal("João Silva", cliente.Nome);
        Assert.Equal("12345678909", cliente.Documento);
        Assert.Equal(TipoDocumentoCliente.Cpf, cliente.TipoDocumento);
        Assert.True(cliente.DataCriacao >= antes);
        Assert.True(cliente.DataCriacao <= DateTime.UtcNow);
        Assert.Empty(cliente.Veiculos);
    }

    [Fact]
    public void Inserir_GeraIdUnico_CadaInstancia()
    {
        var c1 = new Cliente();
        var c2 = new Cliente();
        c1.Inserir("A", DocumentoCliente.Criar("12345678909"));
        c2.Inserir("B", DocumentoCliente.Criar("98765432100"));

        Assert.NotEqual(c1.Id, c2.Id);
    }

    [Fact]
    public void Atualizar_QuandoNomeValido_AlteraNome()
    {
        var cliente       = ClienteValido();
        var idOriginal    = cliente.Id;
        var docOriginal   = cliente.Documento;

        cliente.Atualizar("Novo Nome");

        Assert.Equal("Novo Nome", cliente.Nome);
        Assert.Equal(idOriginal, cliente.Id);
        Assert.Equal(docOriginal, cliente.Documento);
    }

    [Fact]
    public void Atualizar_NaoAlteraDocumento()
    {
        var cliente = ClienteValido();
        cliente.Atualizar("Outro Nome");

        Assert.Equal("12345678909", cliente.Documento);
        Assert.Equal(TipoDocumentoCliente.Cpf, cliente.TipoDocumento);
    }

    // ── DocumentoCliente ─────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    public void DocumentoCliente_QuandoDocumentoInvalido_LancaDomainException(string? documento)
    {
        Assert.Throws<DomainException>(() => DocumentoCliente.Criar(documento!));
    }

    [Fact]
    public void DocumentoCliente_QuandoCpfValido_CriaComTipoCpf()
    {
        var doc = DocumentoCliente.Criar("12345678909");

        Assert.Equal("12345678909", doc.Numero);
        Assert.Equal(TipoDocumentoCliente.Cpf, doc.Tipo);
    }

    [Fact]
    public void DocumentoCliente_QuandoCnpjValido_CriaComTipoCnpj()
    {
        var doc = DocumentoCliente.Criar("11222333000181");

        Assert.Equal("11222333000181", doc.Numero);
        Assert.Equal(TipoDocumentoCliente.Cnpj, doc.Tipo);
    }

    [Fact]
    public void DocumentoCliente_RemoveMascaraAntesDeCriar()
    {
        var doc = DocumentoCliente.Criar("123.456.789-09");

        Assert.Equal("12345678909", doc.Numero);
    }

    [Theory]
    [InlineData("00000000000")]   // todos iguais
    [InlineData("11111111111")]
    public void DocumentoCliente_QuandoCpfComDigitosRepetidos_LancaDomainException(string cpf)
    {
        Assert.Throws<DomainException>(() => DocumentoCliente.Criar(cpf));
    }

    [Fact]
    public void DocumentoCliente_QuandoCpfComDigitosInvalidos_LancaDomainException()
    {
        Assert.Throws<DomainException>(() => DocumentoCliente.Criar("12345678900"));
    }

    [Fact]
    public void DocumentoCliente_Igualdade_QuandoMesmoNumero_SaoIguais()
    {
        var doc1 = DocumentoCliente.Criar("12345678909");
        var doc2 = DocumentoCliente.Criar("12345678909");

        Assert.Equal(doc1, doc2);
    }

    // ── Helpers ──────────────────────────────────────────────────

    private static Cliente ClienteValido(
        string nome = "Cliente Teste",
        string documento = "12345678909")
    {
        var cliente = new Cliente();
        cliente.Inserir(nome, DocumentoCliente.Criar(documento));
        return cliente;
    }
}