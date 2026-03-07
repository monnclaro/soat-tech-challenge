/*namespace SoatTechChallenge.OrdensServico;

public class OrdemServico
{
    public Guid Id { get; private set; }
    public Guid ClienteId { get; private set; }
    public Guid VeiculoId { get; private set; }
    public StatusOS Status { get; private set; }
    public decimal ValorTotal { get; private set; }
    public DateTime CriadaEm { get; private set; }
    public DateTime? FinalizadaEm { get; private set; }
    public string? Observacoes { get; private set; }

    public IReadOnlyCollection<ItemServico> Servicos => _servicos.AsReadOnly();
    public IReadOnlyCollection<ItemPeca> Pecas => _pecas.AsReadOnly();

    private readonly List<ItemServico> _servicos = new();
    private readonly List<ItemPeca> _pecas = new();

    protected OrdemServico() { }

    public OrdemServico(Guid clienteId, Guid veiculoId, string? observacoes = null)
    {
        Id = Guid.NewGuid();
        ClienteId = clienteId;
        VeiculoId = veiculoId;
        Status = StatusOS.Recebida;
        CriadaEm = DateTime.UtcNow;
        Observacoes = observacoes;
    }

    public void AdicionarServico(Servico servico, int quantidade = 1)
    {
        var item = new ItemServico(Id, servico.Id, servico.Nome,
                                   servico.Preco, quantidade);
        _servicos.Add(item);
        RecalcularTotal();
    }

    public void AdicionarPeca(Peca peca, int quantidade)
    {
        if (peca.QuantidadeEstoque < quantidade)
            throw new DomainException("Estoque insuficiente para a peça solicitada.");

        var item = new ItemPeca(Id, peca.Id, peca.Nome, peca.Preco, quantidade);
        _pecas.Add(item);
        RecalcularTotal();
    }

    public void AvancarStatus()
    {
        Status = Status switch
        {
            StatusOS.Recebida           => StatusOS.EmDiagnostico,
            StatusOS.EmDiagnostico      => StatusOS.AguardandoAprovacao,
            StatusOS.AguardandoAprovacao => StatusOS.EmExecucao,
            StatusOS.EmExecucao         => StatusOS.Finalizada,
            StatusOS.Finalizada         => StatusOS.Entregue,
            _ => throw new DomainException("Status inválido para avanço.")
        };

        if (Status == StatusOS.Finalizada)
            FinalizadaEm = DateTime.UtcNow;
    }

    public void AprovarOrcamento()
    {
        if (Status != StatusOS.AguardandoAprovacao)
            throw new DomainException("OS não está aguardando aprovação.");
        Status = StatusOS.EmExecucao;
    }

    private void RecalcularTotal()
    {
        ValorTotal = _servicos.Sum(s => s.Subtotal)
                   + _pecas.Sum(p => p.Subtotal);
    }
}

public enum StatusOS
{
    Recebida,
    EmDiagnostico,
    AguardandoAprovacao,
    EmExecucao,
    Finalizada,
    Entregue
}*/