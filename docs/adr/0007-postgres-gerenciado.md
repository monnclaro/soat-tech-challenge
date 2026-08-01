# ADR 0007 — Postgres Gerenciado (RDS) em Vez de StatefulSet no Cluster

**Status:** Aceito

## Contexto

Desde a Fase 2, o Postgres rodava como `StatefulSet` dentro do próprio cluster Kubernetes (`k8s/postgres/`), com um PVC de 1Gi. A Fase 3 exige "Banco de Dados Gerenciado (PostgreSQL, MySQL, SQL Server, etc.)".

## Decisão

Migrar para **Amazon RDS for PostgreSQL**, provisionado pelo repositório `infra-database`. O `StatefulSet`, `Service`, `Secret` e `PVC` de Postgres foram removidos deste repositório (`k8s/postgres/`) — a aplicação agora se conecta a um endpoint RDS via `ConnectionStrings__Default`, populado pelo CI/CD a partir do SSM Parameter Store (`SecureString` — ver [ADR 0008](./0008-prioridade-de-custo-aws-academy.md)).

## Alternativas descartadas

- **Manter StatefulSet no EKS**: atenderia tecnicamente a "banco gerenciado" apenas se interpretado de forma frouxa (o cluster em si é "gerenciado" pela AWS via EKS) — mas não é o que o requisito pede: backup automático, patch de versão, failover e Multi-AZ sem operação manual são exatamente o que RDS entrega e um StatefulSet não, sem trabalho adicional considerável (Velero para backup, operadores para HA, etc.).
- **Aurora PostgreSQL (Serverless v2)**: mais caro e com um modelo de billing mais complexo para o porte deste projeto (uma oficina, não uma aplicação de escala variável extrema); RDS clássico atende com menos peças.

## Consequências

- Backup automático (`backup_retention_period`, `infra-database/main.tf`) e Multi-AZ opcional (`multi_az`, ligado apenas em produção) substituem qualquer estratégia manual de backup do PVC.
- O banco fica isolado em subnets privadas, sem exposição pública — ver [ADR 0003](./0003-vpc-unica-no-infra-k8s.md).
- Migrations EF Core continuam rodando normalmente (`InitializeDatabaseAsync` no startup) — RDS é PostgreSQL padrão, sem incompatibilidade com o provider Npgsql já usado.
