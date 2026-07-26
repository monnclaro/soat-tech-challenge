using Domain.Common.Events;
using Domain.OrdensServico.Enums;

namespace Domain.OrdensServico.Events;

// Sem duração calculada aqui de propósito: o timestamp da própria transição
// (OcurredAt) já é suficiente — o tempo em cada fase é derivado no New Relic
// a partir da sequência de logs (ver OrdemServicoStatusAlteradoLogHandler e o
// widget_funnel em infra-k8s/newrelic-dashboard.tf), sem precisar persistir
// um timestamp de "início da fase atual" na entidade.
public sealed record OrdemServicoStatusAlteradoDomainEvent(
    Guid IdOrdemServico,
    StatusOrdemServico Status
) : IDomainEvent
{
    public DateTime OcurredAt { get; } = DateTime.UtcNow;
}
