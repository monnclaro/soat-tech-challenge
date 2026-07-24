# Observabilidade

## O que está instrumentado

| Requisito | Como é atendido |
|---|---|
| Latência das APIs | New Relic APM (init container `newrelic-dotnet-init`, sem alteração de código) — dashboard "APIs e Kubernetes" no repo infra-k8s |
| Consumo de CPU/memória do Kubernetes | New Relic Kubernetes integration (`nri-bundle`, infra-k8s/newrelic.tf) — `K8sNodeSample`/`K8sPodSample` |
| Healthchecks e uptime | Endpoint `GET /health` (`src/Api/Program.cs`), usado por readiness/liveness probe do K8s. Sem ALB (ver [ADR 0008](./adr/0008-prioridade-de-custo-aws-academy.md)), "uptime" no New Relic é medido por proxy — contagem de pods prontos + taxa de sucesso das transações do APM (infra-k8s/newrelic-alerts.tf) |
| Alertas para falhas no processamento de OS | `newrelic_nrql_alert_condition` sobre taxa de erro das transactions `*OrdemServico*` (infra-k8s/newrelic-alerts.tf) |
| Logs estruturados (JSON) com correlação | Serilog + `RenderedCompactJsonFormatter` + `CorrelationIdMiddleware` (`X-Correlation-Id`), coletados pelo Fluent Bit do `nri-bundle` |
| Dashboard: volume diário de OS | Widget `widget_bar` no dashboard "Ordens de Serviço" (infra-k8s/newrelic-dashboard.tf) |
| Dashboard: erros nas integrações | Widget sobre erros do `OrcamentoWebhookController` |

## Gaps conhecidos

### Tempo médio de execução por status (Diagnóstico, Execução, Finalização)

A entidade `OrdemServico` (`src/Domain/OrdensServico/OrdemServico.cs`) já guarda `DataCriacao`, `DataInicioExecucao` e `DataFinalizacao`, mas nenhum código hoje emite essas durações como métrica — o widget correspondente no dashboard do New Relic (`newrelic-dashboard.tf`) está montado sobre um evento customizado (`OrdemServicoStatusAlterado`) que ainda **não é emitido pela aplicação**.

Instrumentação necessária para fechar esse gap:
1. Emitir o evento customizado (via `NewRelic.Api.Agent`, `RecordCustomEvent`) no ponto em que `OrdemServico.Finalizar()` é chamado, com `duracaoSegundos = DataFinalizacao - DataInicioExecucao` e `duracaoSegundos = DataInicioExecucao - DataCriacao` (diagnóstico).
2. ~~Pré-requisito: consertar o registro de DI dos `IDomainEventHandler<T>`~~ — corrigido, ver seção abaixo. O widget ainda depende do passo 1 (emissão do evento) para funcionar.

## Correlação de logs

`CorrelationIdMiddleware` (`src/Api/Middlewares/CorrelationIdMiddleware.cs`) lê/gera `X-Correlation-Id` por requisição e o injeta no `LogContext` do Serilog — todo log emitido durante o processamento daquela requisição carrega o mesmo `CorrelationId`, permitindo reconstruir o fluxo completo de uma chamada (incluindo através do `OcorrenciaWebhookController` e chamadas subsequentes) nos logs estruturados coletados pelo New Relic.
