# Diagrama de Componentes (Visão de Nuvem)

Visão C4 (nível de container) da solução na Fase 3: API Gateway, autenticação serverless, cluster Kubernetes, banco gerenciado e observabilidade, através dos 4 repositórios. Infraestrutura ajustada para AWS Academy — sem ALB, sem NAT, sem Secrets Manager (ver [ADR 0008](./adr/0008-prioridade-de-custo-aws-academy.md)).

```mermaid
flowchart TB
    subgraph Cliente["Cliente / Front-end"]
        Usuario["Funcionário (Usuario)"]
        ClienteOficina["Cliente da oficina"]
    end

    subgraph AWS["AWS (us-east-1) — conta AWS Academy"]
        subgraph GW["API Gateway — repo lambda"]
            APIGW["HTTP API"]
            AuthFn["Lambda: AuthFunction\n(valida CPF, consulta Cliente, emite JWT)"]
            AuthzFn["Lambda: AuthorizerFunction\n(valida JWT nas rotas /api/*)"]
        end

        subgraph EKS["Amazon EKS — repo infra-k8s + soat-tech-challenge"]
            Node["Node (subnet pública, IP próprio — sem NAT)"]
            Pods["soat-api (Deployment, HPA 1-4 réplicas, NodePort 30080)"]
        end

        RDS["Amazon RDS PostgreSQL — repo infra-database"]
        SSM["SSM Parameter Store\n(config cross-repo + segredos SecureString)"]

        subgraph NR["New Relic — repo infra-k8s"]
            APM["APM (init container)"]
            K8sInt["Kubernetes integration (nri-bundle)"]
            Dash["Dashboards"]
            Alerts["Alertas (e-mail)"]
        end
    end

    Usuario -->|"POST /api/auth/login (email/senha)"| APIGW
    ClienteOficina -->|"POST /auth/login-cpf"| APIGW
    APIGW -->|"rota pública"| AuthFn
    AuthFn --> RDS
    AuthFn --> SSM

    Usuario -->|"Bearer JWT, /api/*"| APIGW
    ClienteOficina -->|"Bearer JWT, /api/*"| APIGW
    APIGW -->|"Lambda Authorizer"| AuthzFn
    AuthzFn --> SSM
    APIGW -->|"HTTP_PROXY p/ IP do node:30080, se autorizado"| Node
    Node --> Pods
    Pods --> RDS
    Pods --> SSM

    Pods -.->|logs JSON + métricas| APM
    K8sInt -.->|CPU/memória dos nodes/pods| NR
    APM --> Dash
    K8sInt --> Dash
    Dash --> Alerts

    SSM -. "vpc-id, subnets, endpoints" .- EKS
    SSM -. "rds endpoint/username/password" .- RDS
    SSM -. "node-ip" .- GW
```

## Mapeamento repositório → componente

| Componente no diagrama | Repositório |
|---|---|
| API Gateway, AuthFunction, AuthorizerFunction | [soat-tech-challenge-lambda](https://github.com/monnclaro/soat-tech-challenge-lambda) |
| VPC, EKS, New Relic (integração + dashboards + alertas) | [soat-tech-challenge-infra-k8s](https://github.com/monnclaro/soat-tech-challenge-infra-k8s) |
| RDS, credenciais em SSM SecureString | [soat-tech-challenge-infra-database](https://github.com/monnclaro/soat-tech-challenge-infra-database) |
| Deployment/Service (NodePort)/HPA `soat-api`, código da API | [soat-tech-challenge](https://github.com/monnclaro/soat-tech-challenge) (este repositório) |

## Diagramas complementares

- Diagrama de sequência — autenticação por CPF: [sequence-auth-cpf.md](./sequence-auth-cpf.md)
- Diagrama de sequência — abertura de ordem de serviço: [sequence-abertura-os.md](./sequence-abertura-os.md)
- Diagrama entidade-relacionamento: [der.md](./der.md)
- RFCs: [rfcs/](./rfcs)
- ADRs: [adr/](./adr) — em especial [0008](./adr/0008-prioridade-de-custo-aws-academy.md) para as decisões de custo/AWS Academy
