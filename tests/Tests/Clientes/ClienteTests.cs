using Domain.Clientes;
using Domain.Clientes.Enums;
using Domain.Common.Exceptions;
using Xunit;

namespace Tests.Clientes;

public class ClienteTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Inserir_QuandoNomeInvalido_LancaDomainException(string? nome)
    {
        var cliente = new Cliente();
        Assert.Throws<DomainException>(() =>
            cliente.Inserir(nome!, "12345678900", TipoDocumentoCliente.Cpf));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Inserir_QuandoDocumentoInvalido_LancaDomainException(string? documento)
    {
        var cliente = new Cliente();
        Assert.Throws<DomainException>(() =>
            cliente.Inserir("João Silva", documento!, TipoDocumentoCliente.Cpf));
    }

    [Fact]
    public void Inserir_QuandoDadosValidos_PopulaPropriedadesCorretamente()
    {
        var antes = DateTime.UtcNow;
        var cliente = new Cliente();

        cliente.Inserir("João Silva", "12345678900", TipoDocumentoCliente.Cpf);

        Assert.NotEqual(Guid.Empty, cliente.Id);
        Assert.Equal("João Silva", cliente.Nome);
        Assert.Equal("12345678900", cliente.Documento);
        Assert.Equal(TipoDocumentoCliente.Cpf, cliente.TipoDocumento);
        Assert.True(cliente.DataCriacao >= antes);
        Assert.True(cliente.DataCriacao <= DateTime.UtcNow);
        Assert.Empty(cliente.Veiculos);
    }

    [Theory]
    [InlineData(TipoDocumentoCliente.Cpf)]
    [InlineData(TipoDocumentoCliente.Cnpj)]
    public void Inserir_QuandoTipoDocumentoDiferente_PersisteTipoCorreto(TipoDocumentoCliente tipo)
    {
        var cliente = new Cliente();
        cliente.Inserir("Empresa X", "12345678000199", tipo);

        Assert.Equal(tipo, cliente.TipoDocumento);
    }

    [Fact]
    public void Inserir_GeraIdUnico_CadaInstancia()
    {
        var c1 = new Cliente();
        var c2 = new Cliente();
        c1.Inserir("A", "111", TipoDocumentoCliente.Cpf);
        c2.Inserir("B", "222", TipoDocumentoCliente.Cpf);

        Assert.NotEqual(c1.Id, c2.Id);
    }

    [Fact]
    public void Atualizar_QuandoNomeValido_AlteraNome()
    {
        var cliente = ClienteValido();
        var idOriginal = cliente.Id;
        var documentoOriginal = cliente.Documento;

        cliente.Atualizar("Novo Nome");

        Assert.Equal("Novo Nome", cliente.Nome);
        // Campos imutáveis não devem mudar
        Assert.Equal(idOriginal, cliente.Id);
        Assert.Equal(documentoOriginal, cliente.Documento);
    }

    [Fact]
    public void Atualizar_NaoAlteraDocumentoNemTipoDocumento()
    {
        var cliente = ClienteValido();
        cliente.Atualizar("Outro Nome");

        Assert.Equal("12345678900", cliente.Documento);
        Assert.Equal(TipoDocumentoCliente.Cpf, cliente.TipoDocumento);
    }

   #region Helpers

    private static Cliente ClienteValido(
        string nome = "Cliente Teste",
        string documento = "12345678900",
        TipoDocumentoCliente tipo = TipoDocumentoCliente.Cpf)
    {
        var cliente = new Cliente();
        cliente.Inserir(nome, documento, tipo);
        return cliente;
    }
    
    #endregion
}