# RFC 0003 — Estratégia de Autenticação

**Status:** Aceito
**Contexto:** Fase 3 exige proteger rotas sensíveis com autenticação via CPF, através de uma função serverless que valida o CPF, consulta existência/status do cliente e emite um JWT.

## Problema

Hoje o único mecanismo de autenticação é `Usuario` (funcionário) logando com email/senha (`POST /api/auth/login`), emitindo um JWT validado pela própria API. Clientes da oficina não têm nenhuma forma de se autenticar — todas as rotas hoje exigem role `Admin` (funcionário) ou não exigem autenticação nenhuma (bug encontrado nesta fase, ver [OrdemServicoStatusController](../../src/Api/Controllers/OrdensServico/OrdemServicoStatusController.cs), que não tinha `[Authorize]` algum antes desta entrega).

## Alternativas consideradas

| Opção | Prós | Contras |
|---|---|---|
| **CPF + Lambda emite JWT** (escolhida) | Aderente ao enunciado; sem senha para o cliente memorizar/vazar; desacopla auth de cliente da API principal | CPF sozinho é uma identificação fraca (qualquer um com o CPF de outra pessoa "loga") |
| CPF + senha (cadastro de senha para clientes) | Autenticação mais forte | Fora do escopo pedido; exige fluxo de cadastro/recuperação de senha para clientes, que não existe hoje |
| OAuth/OIDC de terceiro (Google, etc.) | Padrão robusto, delega gestão de credenciais | Não atende ao requisito explícito de "autenticação via CPF"; exige que todo cliente tenha conta em provedor externo |

## Decisão

CPF like-for-like com o enunciado: o cliente informa o CPF em `POST /auth/login-cpf` (API Gateway → Lambda `AuthFunction`), que valida o formato/dígitos verificadores, consulta `Cliente` por `Documento` no RDS, checa `Ativo`, e devolve um JWT HS256 com claim `Role=Cliente` — mesmo formato aceito pelo `AddJwtAuthentication` já existente na API. Documentado o trade-off: **CPF isoladamente não prova posse de identidade** (não é uma senha), então esta é uma autenticação de baixa fricção adequada ao contexto (autoatendimento de baixo risco — consultar status de OS), não recomendada para operações sensíveis (essas continuam exigindo `Usuario`/`Admin`).

## Consequências

- Duas populações de token coexistem: `Usuario` (funcionário, email/senha, emitido pela API) e `Cliente` (CPF, emitido pelo Lambda) — diferenciadas pela claim `Role`.
- Rotas continuam decidindo autorização por `[Authorize(Roles = "...")]`, sem necessidade de um mecanismo novo de autorização.
- O segredo JWT passa a ser compartilhado entre dois repositórios (app e lambda) via SSM Parameter Store (`SecureString`) — ver [ADR 0005](../adr/0005-jwt-simetrico-compartilhado.md) e [ADR 0008](../adr/0008-prioridade-de-custo-aws-academy.md).
- Nenhuma rota de "criar senha" ou "recuperar senha" foi adicionada para clientes — está fora do escopo desta fase.
