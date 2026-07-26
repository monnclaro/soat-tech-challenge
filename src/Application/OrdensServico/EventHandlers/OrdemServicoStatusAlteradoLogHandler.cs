using Application.Common.Interfaces;
using Domain.OrdensServico.Events;
using Microsoft.Extensions.Logging;

namespace Application.OrdensServico.EventHandlers;

// Em vez de um evento customizado via NewRelic.Api.Agent (RecordCustomEvent),
// emite um log estruturado a cada transição de status — os logs já são
// coletados pelo Fluent Bit do nri-bundle (ver infra-k8s/newrelic.tf) e o New
// Relic parseia automaticamente linhas de log em JSON, promovendo cada
// propriedade nomeada do template a um atributo consultável via NRQL
// (FROM Log). Sem dependência nova de agente.
//
// Não calcula duração aqui: o timestamp de cada log (já registrado pelo New
// Relic na ingestão) é suficiente — o tempo entre fases é derivado no New
// Relic via funnel() sobre a sequência de transições por idOrdemServico (ver
// o widget_funnel em infra-k8s/newrelic-dashboard.tf), sem precisar persistir
// um timestamp de "início da fase atual" na entidade.
internal sealed class OrdemServicoStatusAlteradoLogHandler : IDomainEventHandler<OrdemServicoStatusAlteradoDomainEvent>
{
    private readonly ILogger<OrdemServicoStatusAlteradoLogHandler> _logger;

    public OrdemServicoStatusAlteradoLogHandler(ILogger<OrdemServicoStatusAlteradoLogHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrdemServicoStatusAlteradoDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "OrdemServicoStatusAlterado {idOrdemServico} {status}",
            domainEvent.IdOrdemServico,
            domainEvent.Status);

        return Task.CompletedTask;
    }
}
