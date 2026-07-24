# Diagrama de Sequência — Autenticação por CPF

Fluxo completo: cliente informa o CPF, recebe um JWT, e usa esse token para acessar uma rota protegida (ex.: consultar status de uma ordem de serviço). Segredos lidos do SSM Parameter Store, exposição via NodePort — sem Secrets Manager nem ALB (ver [ADR 0008](./adr/0008-prioridade-de-custo-aws-academy.md)).

```mermaid
sequenceDiagram
    actor Cliente
    participant GW as API Gateway (HTTP API)
    participant Auth as Lambda AuthFunction
    participant RDS as RDS PostgreSQL
    participant Authz as Lambda Authorizer
    participant Node as Node EKS (NodePort 30080)
    participant API as soat-api

    Cliente->>GW: POST /auth/login-cpf { cpf }
    GW->>Auth: invoke (proxy)
    Auth->>Auth: CpfValidator.TryNormalizar(cpf)
    alt CPF com formato inválido
        Auth-->>GW: 400 { erro: "CPF inválido" }
        GW-->>Cliente: 400
    else CPF válido
        Note over Auth: credenciais do RDS lidas do SSM (env var, Terraform)
        Auth->>RDS: SELECT id, nome, ativo FROM cliente WHERE documento = ?
        alt cliente não encontrado
            Auth-->>GW: 404 { erro: "Cliente não encontrado" }
            GW-->>Cliente: 404
        else cliente inativo
            Auth-->>GW: 403 { erro: "Cliente inativo" }
            GW-->>Cliente: 403
        else cliente ativo
            Note over Auth: segredo JWT lido do SSM (env var, Terraform)
            Auth->>Auth: JwtService.GerarTokenCliente(id, nome)
            Auth-->>GW: 200 { token }
            GW-->>Cliente: 200 { token }
        end
    end

    Note over Cliente,API: Cliente agora usa o token nas próximas chamadas

    Cliente->>GW: GET /api/v1/ordens-servico/{id}/status<br/>Authorization: Bearer {token}
    GW->>Authz: invoke (Lambda Authorizer, REQUEST simple response)
    Authz->>Authz: JwtService.ValidarToken(token) — segredo via env var (SSM)
    alt token inválido/expirado
        Authz-->>GW: { isAuthorized: false }
        GW-->>Cliente: 401
    else token válido
        Authz-->>GW: { isAuthorized: true, context: { role: "Cliente", sub } }
        GW->>Node: HTTP_PROXY /api/v1/ordens-servico/{id}/status<br/>(IP público do node : 30080)
        Node->>API: encaminha requisição (Service NodePort)
        API->>API: [Authorize(Roles = "Cliente,Admin")]
        API-->>Node: 200 { status }
        Node-->>GW: 200
        GW-->>Cliente: 200 { status }
    end
```
