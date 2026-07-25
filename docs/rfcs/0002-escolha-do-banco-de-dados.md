# RFC 0002 — Escolha do Banco de Dados e Ajustes no Modelo Relacional

**Status:** Aceito
**Contexto:** Fase 3 exige banco de dados gerenciado (PostgreSQL, MySQL ou SQL Server) e "justificativa formal para a escolha do banco de dados e ajustes no modelo relacional".

## Problema

O sistema já roda em PostgreSQL desde a Fase 1 (containerizado) e Fase 2 (StatefulSet no Kubernetes). A Fase 3 pede um banco **gerenciado** — decidir se trocamos de motor ou apenas de forma de operação, e revisar o modelo para as novas necessidades (segunda forma de login do `Usuario` via CPF).

## Alternativas consideradas

| Critério | PostgreSQL (RDS) | MySQL (RDS) | SQL Server (RDS) |
|---|---|---|---|
| Continuidade com o schema/migrations existentes (EF Core + Npgsql) | Zero atrito | Reescrever provider EF Core, tipos, migrations | Reescrever provider, licenciamento |
| Suporte a `citext`/constraints usados no domínio (ex.: unicidade de `documento`) | Nativo | Equivalente via `COLLATE` | Nativo |
| Custo de licença | Nenhum | Nenhum | Licenciamento SQL Server (mesmo no RDS) |
| Familiaridade da equipe | Alta (usado desde a Fase 1) | Baixa | Baixa |

## Decisão

**Manter PostgreSQL, migrando de StatefulSet para Amazon RDS for PostgreSQL 16.** Trocar de motor de banco no meio do projeto adicionaria risco (reescrever migrations, tipos, queries) sem nenhum ganho correspondente — nenhum requisito da Fase 3 depende de um motor específico. RDS elimina a operação manual de backup/patch/HA que o StatefulSet exigia.

## Ajustes no modelo relacional desta fase

| Mudança | Motivo |
|---|---|
| `Usuario.Cpf` (`varchar(11)`, único, obrigatório) + `Usuario.Ativo` (`boolean`, default `true`) | O Lambda de autenticação por CPF precisa checar "existência e status" (requisito explícito da Fase 3) de quem está logando — quem ganha uma segunda forma de login é o `Usuario` (protege rotas sensíveis), não o `Cliente` — ver [RFC 0003](./0003-estrategia-de-autenticacao.md) |
| `Cliente.Ativo` (`boolean`, default `true`) | Adicionada por completude de cadastro nesta mesma entrega, mas **não é usada em autenticação** — `Cliente` não loga no sistema |
| Índice único em `Usuario.Cpf` | Necessário para a busca por CPF no Lambda ser O(log n) via índice, não scan |

Diagrama ER completo e relacionamentos: [der.md](./der.md).

## Consequências

- Novas migrations EF Core (`AdicionandoClienteAtivo`, `AdicionandoCpfEAtivoAoUsuario`) — aplicadas automaticamente pelo `InitializeDatabaseAsync` no startup da API (padrão já existente no projeto).
- O Lambda de autenticação acessa a mesma tabela `usuario` via Npgsql direto (não via EF Core) — decisão registrada à parte no repositório lambda, já que é uma unidade de deploy diferente.
- RDS fica isolado em subnets privadas, acessível só de dentro da VPC (EKS e Lambda) — nunca exposto publicamente (`publicly_accessible = false` em `infra-database/main.tf`).
