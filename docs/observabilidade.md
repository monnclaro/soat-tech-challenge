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
| Dashboard: tempo por fase (funil de status) | Widget `widget_funnel` sobre `FROM Log` (infra-k8s/newrelic-dashboard.tf) — ver "Tempo por fase" abaixo |

## Tempo por fase (Recebida, Diagnóstico, Aguardando Aprovação, Execução, Finalizada, Entregue)

`OrdemServico` (`src/Domain/OrdensServico/OrdemServico.cs`) levanta `OrdemServicoStatusAlteradoDomainEvent` (`src/Domain/OrdensServico/Events`) em **todas as 6 transições de status**: `Inserir` (Recebida), `IniciarDiagnostico` (EmDiagnostico), `FinalizarDiagnostico`/`EnviarOrcamento` (AguardandoAprovacao), `AprovarOrcamento` (EmExecucao), `Finalizar` (Finalizada) e `Entregar` (Entregue). O evento carrega só `IdOrdemServico` e o `Status` que acabou de começar — **nenhuma duração é calculada no código**.

`OrdemServicoStatusAlteradoLogHandler` (`src/Application/OrdensServico/EventHandlers/OrdemServicoStatusAlteradoLogHandler.cs`), resolvido pelo `DomainEventsDispatcher` como qualquer outro `IDomainEventHandler<T>`, loga cada transição de forma estruturada:

```
_logger.LogInformation("OrdemServicoStatusAlterado {idOrdemServico} {status}", ...)
```

**Por que log em vez de `RecordCustomEvent`**: os logs já são coletados pelo Fluent Bit embutido no `nri-bundle` (`logging.enabled = true` em `infra-k8s/newrelic.tf`) — o mesmo caminho que já traz todo log estruturado da aplicação para o New Relic (Serilog + `RenderedCompactJsonFormatter`, ver `src/Api/Program.cs`). O New Relic parseia automaticamente linhas de log em JSON e promove cada propriedade nomeada do template (`idOrdemServico`, `status`) a um atributo do evento `Log`, consultável via NRQL — sem precisar adicionar o pacote `NewRelic.Api.Agent` nem instrumentar a aplicação para falar diretamente com a API do New Relic.

**Por que nenhuma duração é calculada em código**: pra medir "quanto tempo a OS ficou em EmDiagnostico" seria preciso saber quando aquela fase começou — e isso não é derivável dos campos já existentes (`DataCriacao`/`DataInicioExecucao`/`DataFinalizacao` só cobrem 3 dos 6 status). A alternativa seria persistir um novo timestamp genérico ("início da fase atual") na entidade, mas isso foi descartado deliberadamente — a duração por fase é calculada inteiramente no New Relic, correlacionando os logs de transição por `idOrdemServico` via `funnel()` (NRQL), usando só o timestamp de ingestão de cada log. O widget correspondente (`widget_funnel`) consulta:

```sql
SELECT funnel(timestamp,
  WHERE status = 'Recebida' AS 'Recebida',
  WHERE status = 'EmDiagnostico' AS 'EmDiagnostico',
  WHERE status = 'AguardandoAprovacao' AS 'AguardandoAprovacao',
  WHERE status = 'EmExecucao' AS 'EmExecucao',
  WHERE status = 'Finalizada' AS 'Finalizada',
  WHERE status = 'Entregue' AS 'Entregue'
) FROM Log WHERE idOrdemServico IS NOT NULL FACET idOrdemServico SINCE 30 days ago
```

**Nota**: essa query é best-effort — até esta entrega nenhum ambiente foi aplicado na AWS, então não há logs reais no New Relic pra validar contra a UI. `terraform validate` confirma que `widget_funnel` é um bloco válido no provider, mas a semântica exata do NRQL (nomes de atributo, formato de saída do `funnel()`) só pode ser conferida depois do primeiro deploy real — ajustar se necessário nesse momento.

## Correlação de logs

`CorrelationIdMiddleware` (`src/Api/Middlewares/CorrelationIdMiddleware.cs`) lê/gera `X-Correlation-Id` por requisição e o injeta no `LogContext` do Serilog — todo log emitido durante o processamento daquela requisição carrega o mesmo `CorrelationId`, permitindo reconstruir o fluxo completo de uma chamada (incluindo através do `OcorrenciaWebhookController` e chamadas subsequentes) nos logs estruturados coletados pelo New Relic.
