# Diagrama de Sequência — Autenticação por CPF

Fluxo completo: um funcionário (`Usuario`) informa o CPF como segunda forma de login (alternativa ao email/senha já existente), recebe um JWT, e usa esse token para acessar uma rota sensível do back-office (ex.: consultar status de uma ordem de serviço). Segredos lidos do SSM Parameter Store, exposição via NodePort — sem Secrets Manager nem ALB (ver [ADR 0008](./adr/0008-prioridade-de-custo-aws-academy.md)). Sem Lambda Authorizer: o JWT é validado pela própria API, não no Gateway (ver [ADR 0009](./adr/0009-sem-lambda-authorizer.md)).

```mermaid
sequenceDiagram
    actor Func as Funcionário (Usuario)
    participant GW as API Gateway (HTTP API)
    participant Auth as Lambda AuthFunction
    participant RDS as RDS PostgreSQL
    participant Node as Node EKS (NodePort 30080)
    participant API as soat-api

    Func->>GW: POST /auth/login-cpf { cpf }
    GW->>Auth: invoke (proxy)
    Auth->>Auth: CpfValidator.TryNormalizar(cpf)
    alt CPF com formato inválido
        Auth-->>GW: 400 { erro: "CPF inválido" }
        GW-->>Func: 400
    else CPF válido
        Note over Auth: credenciais do RDS lidas do SSM (env var, Terraform)
        Auth->>RDS: SELECT id, nome, ativo, role FROM usuario u<br/>LEFT JOIN usuario_role r ON r.id_usuario = u.id<br/>WHERE u.cpf = ?
        alt usuário não encontrado
            Auth-->>GW: 404 { erro: "Usuário não encontrado" }
            GW-->>Func: 404
        else usuário inativo
            Auth-->>GW: 403 { erro: "Usuário inativo" }
            GW-->>Func: 403
        else usuário ativo
            Note over Auth: segredo JWT lido do SSM (env var, Terraform)
            Auth->>Auth: JwtService.GerarTokenUsuario(nome, roles)
            Auth-->>GW: 200 { token }
            GW-->>Func: 200 { token }
        end
    end

    Note over Func,API: Token tem o mesmo formato (claims Name/Role) emitido<br/>por POST /api/auth/login (email/senha) — são intercambiáveis

    Func->>GW: GET /api/v1/ordens-servico/{id}/status<br/>Authorization: Bearer {token}
    GW->>Node: HTTP_PROXY /api/v1/ordens-servico/{id}/status<br/>(IP público do node : 30080 — sem authorizer no Gateway)
    Node->>API: encaminha requisição (Service NodePort)
    API->>API: AddJwtAuthentication valida o token (mesmo segredo HS256)
    alt token inválido/expirado
        API-->>Node: 401
        Node-->>GW: 401
        GW-->>Func: 401
    else token válido
        API->>API: [Authorize(Roles = "Admin")]
        API-->>Node: 200 { status }
        Node-->>GW: 200
        GW-->>Func: 200 { status }
    end
```
