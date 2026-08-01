# Documentação — Fase 3

Índice da documentação arquitetural. Este repositório é o hub de documentação para os 4 repositórios da solução.

## Arquitetura

- [architecture.md](./architecture.md) — Diagrama de componentes (C4, visão de nuvem: APIs, banco, monitoramento)
- [sequence-auth-cpf.md](./sequence-auth-cpf.md) — Diagrama de sequência: autenticação por CPF
- [sequence-abertura-os.md](./sequence-abertura-os.md) — Diagrama de sequência: abertura de ordem de serviço
- [der.md](./der.md) — Diagrama entidade-relacionamento e justificativa do modelo de dados
- [infra.md](./infra.md) — Infraestrutura e fluxo de CI/CD desta fase (com histórico da Fase 2)
- [observabilidade.md](./observabilidade.md) — O que está instrumentado no New Relic e gaps conhecidos
- [ci-cd-governance.md](./ci-cd-governance.md) — Environments, proteção de branch e segredos nos 4 repositórios

## RFCs (decisões técnicas)

- [0001 — Escolha da nuvem](./rfcs/0001-escolha-da-nuvem.md)
- [0002 — Escolha do banco de dados](./rfcs/0002-escolha-do-banco-de-dados.md)
- [0003 — Estratégia de autenticação](./rfcs/0003-estrategia-de-autenticacao.md)

## ADRs (decisões arquiteturais)

- [0001 — Split em 4 repositórios](./adr/0001-split-de-repositorios.md)
- [0002 — Comunicação síncrona via API Gateway](./adr/0002-comunicacao-sincrona-via-api-gateway.md)
- [0003 — VPC única no infra-k8s](./adr/0003-vpc-unica-no-infra-k8s.md)
- [0004 — HPA para escalabilidade](./adr/0004-hpa-para-escalabilidade.md)
- [0005 — JWT simétrico compartilhado](./adr/0005-jwt-simetrico-compartilhado.md)
- [0006 — Lambda em .NET 8, independente do app](./adr/0006-lambda-dotnet8-independente-do-app.md)
- [0007 — Postgres gerenciado (RDS)](./adr/0007-postgres-gerenciado.md)
- [0008 — Prioridade de custo e AWS Academy](./adr/0008-prioridade-de-custo-aws-academy.md)
- [0009 — Sem Lambda Authorizer no API Gateway](./adr/0009-sem-lambda-authorizer.md)

## READMEs dos outros repositórios

- [soat-tech-challenge-infra-k8s](https://github.com/monnclaro/soat-tech-challenge-infra-k8s)
- [soat-tech-challenge-infra-database](https://github.com/monnclaro/soat-tech-challenge-infra-database)
- [soat-tech-challenge-lambda](https://github.com/monnclaro/soat-tech-challenge-lambda)
