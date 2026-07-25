# Diagrama de Sequência — Abertura de Ordem de Serviço

Fluxo de um funcionário autenticado abrindo uma ordem de serviço para um cliente/veículo já cadastrados. `POST /api/v1/ordens-servico` exige role `Admin` — abertura de OS é uma operação de back-office, não self-service do cliente (ver [RFC 0003](./rfcs/0003-estrategia-de-autenticacao.md)).

```mermaid
sequenceDiagram
    actor Funcionario as Funcionário (Usuario)
    participant GW as API Gateway
    participant Node as Node EKS (NodePort 30080)
    participant API as soat-api
    participant UC as InserirOrdemServicoUseCase
    participant DB as RDS PostgreSQL
    participant NR as New Relic (APM + Logs)

    Funcionario->>GW: POST /api/v1/ordens-servico<br/>Authorization: Bearer {token Usuario}<br/>{ idCliente, idVeiculo, servicos[], produtos[] }
    GW->>Node: HTTP_PROXY (IP público do node : 30080 — sem authorizer no Gateway)
    Node->>API: encaminha requisição (Service NodePort)

    API->>API: CorrelationIdMiddleware gera/propaga X-Correlation-Id
    API->>API: AddJwtAuthentication valida o token + [Authorize(Roles = "Admin")]
    API->>UC: Execute(InserirOrdemServicoInput)

    UC->>DB: BuscarPorId(idCliente), BuscarPorId(idVeiculo)
    alt cliente ou veículo não encontrado
        UC-->>API: NaoEncontrado()
        API-->>Funcionario: 404
    else dados válidos
        UC->>DB: BuscarPorIds(servicos), BuscarPorIds(produtos)
        UC->>UC: OrdemServico.Inserir(idCliente, idVeiculo, servicos, produtos)<br/>Status = Recebida, ValorTotal calculado
        UC->>DB: INSERT ordem_servico + itens (transação)
        DB-->>UC: OK
        UC-->>API: Ok(OrdemServicoOutput)
        API-->>Node: 201 Created
        Node-->>GW: 201
        GW-->>Funcionario: 201 { id, status: "Recebida", ... }
    end

    API-)NR: log estruturado (JSON) + trace da transação,<br/>correlacionados por X-Correlation-Id
```

## Transições de status subsequentes

Após a abertura (`Recebida`), a OS avança por `EmDiagnostico` → `AguardandoAprovacao` → `EmExecucao` → `Finalizada` → `Entregue`, cada uma via métodos específicos da entidade `OrdemServico` (`src/Domain/OrdensServico/OrdemServico.cs`). A consulta de status (`GET /api/v1/ordens-servico/{id}/status`) é a rota liberada também para clientes (`Cliente,Admin`) — ver [sequence-auth-cpf.md](./sequence-auth-cpf.md) para o fluxo de autenticação que precede essa chamada quando quem consulta é o cliente.
