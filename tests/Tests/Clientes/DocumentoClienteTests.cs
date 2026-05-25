using Domain.Clientes.Enums;
using Domain.Clientes.ValueObjects;
using Domain.Common.Exceptions;

namespace Tests.Clientes;

public class DocumentoClienteTests
{
    // ── Criação inválida ─────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Criar_QuandoVazioOuNulo_LancaDomainException(string? documento)
    {
        Assert.Throws<DomainException>(() => DocumentoCliente.Criar(documento!));
    }

    [Theory]
    [InlineData("abc")]        // sem dígitos válidos
    [InlineData("!!!")]
    public void Criar_QuandoSemDigitos_LancaDomainException(string documento)
    {
        Assert.Throws<DomainException>(() => DocumentoCliente.Criar(documento));
    }

    [Theory]
    [InlineData("1234567")]       // 7 dígitos
    [InlineData("123456789012")]  // 12 dígitos
    public void Criar_QuandoTamanhoInvalido_LancaDomainException(string documento)
    {
        Assert.Throws<DomainException>(() => DocumentoCliente.Criar(documento));
    }

    [Theory]
    [InlineData("00000000000")]  // todos iguais CPF
    [InlineData("11111111111")]
    [InlineData("99999999999999")] // todos iguais CNPJ
    public void Criar_QuandoDigitosRepetidos_LancaDomainException(string documento)
    {
        Assert.Throws<DomainException>(() => DocumentoCliente.Criar(documento));
    }

    [Theory]
    [InlineData("12345678901")]  // dígitos verificadores errados CPF
    [InlineData("11222333000100")] // dígitos verificadores errados CNPJ
    public void Criar_QuandoDigitosVerificadoresInvalidos_LancaDomainException(string documento)
    {
        Assert.Throws<DomainException>(() => DocumentoCliente.Criar(documento));
    }

    // ── CPF válido ───────────────────────────────────────────────

    [Theory]
    [InlineData("52998224725")]
    [InlineData("11144477735")]
    public void Criar_QuandoCpfValido_CriaComTipoCpf(string cpf)
    {
        var doc = DocumentoCliente.Criar(cpf);

        Assert.Equal(cpf, doc.Numero);
        Assert.Equal(TipoDocumentoCliente.Cpf, doc.Tipo);
    }

    [Fact]
    public void Criar_CpfComMascara_NormalizaRemovendoMascara()
    {
        var doc = DocumentoCliente.Criar("529.982.247-25");

        Assert.Equal("52998224725", doc.Numero);
        Assert.Equal(TipoDocumentoCliente.Cpf, doc.Tipo);
    }

    // ── CNPJ válido ──────────────────────────────────────────────

    [Theory]
    [InlineData("11222333000181")]
    public void Criar_QuandoCnpjValido_CriaComTipoCnpj(string cnpj)
    {
        var doc = DocumentoCliente.Criar(cnpj);

        Assert.Equal(cnpj, doc.Numero);
        Assert.Equal(TipoDocumentoCliente.Cnpj, doc.Tipo);
    }

    [Fact]
    public void Criar_CnpjComMascara_NormalizaRemovendoMascara()
    {
        var doc = DocumentoCliente.Criar("11.222.333/0001-81");

        Assert.Equal("11222333000181", doc.Numero);
        Assert.Equal(TipoDocumentoCliente.Cnpj, doc.Tipo);
    }

    // ── Igualdade ────────────────────────────────────────────────

    [Fact]
    public void Igualdade_QuandoMesmoNumero_SaoIguais()
    {
        var doc1 = DocumentoCliente.Criar("52998224725");
        var doc2 = DocumentoCliente.Criar("52998224725");

        Assert.Equal(doc1, doc2);
    }

    [Fact]
    public void Igualdade_QuandoNumeroDiferente_NaoSaoIguais()
    {
        var doc1 = DocumentoCliente.Criar("52998224725");
        var doc2 = DocumentoCliente.Criar("11144477735");

        Assert.NotEqual(doc1, doc2);
    }

    [Fact]
    public void Igualdade_CpfECnpj_NaoSaoIguais()
    {
        var cpf  = DocumentoCliente.Criar("52998224725");
        var cnpj = DocumentoCliente.Criar("11222333000181");

        Assert.NotEqual(cpf, cnpj);
    }
}