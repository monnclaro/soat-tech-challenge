# RFC 0003 — Estratégia de Autenticação

**Status:** Aceito
**Contexto:** Fase 3 exige proteger rotas sensíveis com autenticação via CPF, através de uma função serverless que valida o CPF, consulta existência/status do usuário e emite um JWT.

## Problema

Hoje o único mecanismo de autenticação é `Usuario` (funcionário) logando com email/senha (`POST /api/auth/login`), emitindo um JWT validado pela própria API. O enunciado pede que a proteção das rotas sensíveis passe por uma segunda forma de login via CPF, servida por uma função serverless — não um novo tipo de identidade, mas uma segunda porta de entrada para a mesma identidade (`Usuario`) que já protege o back-office.

Cogitou-se inicialmente modelar isso como login de `Cliente` (autoatendimento do cliente da oficina), mas o enunciado é explícito: o objetivo é proteger **rotas sensíveis**, que hoje só `Usuario`/`Admin` acessa. Não faria sentido um `Cliente` ganhar acesso a rotas administrativas via CPF — quem precisa de uma segunda forma de login é o próprio `Usuario`.

## Alternativas consideradas

| Opção | Prós | Contras |
|---|---|---|
| **CPF do `Usuario` + Lambda emite JWT** (escolhida) | Aderente ao enunciado; protege exatamente as rotas sensíveis que já exigem `Admin`; reaproveita o mesmo formato de token do login por email/senha | CPF sozinho é uma identificação fraca (qualquer um com o CPF de outro funcionário "loga") |
| CPF do `Cliente` + Lambda emite JWT com role própria | Daria autoatendimento a clientes | Não protege rota sensível nenhuma (o objetivo do enunciado); exigiria criar rotas novas de autoatendimento fora do escopo pedido |
| CPF + senha (segundo fator) | Autenticação mais forte que CPF isolado | Fora do escopo pedido; exige fluxo de cadastro de senha adicional que não existe hoje |

## Decisão

O `Usuario` (funcionário) ganha uma segunda forma de login: informa o CPF em `POST /auth/login-cpf` (API Gateway → Lambda `AuthFunction`), que valida o formato/dígitos verificadores, consulta `Usuario` por `Cpf` no RDS, checa `Ativo`, e devolve um JWT HS256 com as mesmas claims (`Name` + uma `Role` por role do usuário, tipicamente `Admin`) emitidas pelo login por email/senha existente na API — **mesmo tipo de token, duas formas de obtê-lo**. Documentado o trade-off: **CPF isoladamente não prova posse de identidade** (não é uma senha), mas é aceitável aqui porque é uma segunda credencial de conveniência para quem já é `Usuario` cadastrado e ativo, não um novo nível de acesso.

`Cliente` ganha uma propriedade `Ativo` nesta mesma entrega (útil para o cadastro em si), mas **não participa da autenticação** — não há login de `Cliente` no sistema.

## Consequências

- Uma única população de token (`Usuario`, role `Admin`), emitida por dois caminhos: email/senha (API) e CPF (Lambda) — o token resultante é indistinguível entre os dois, então nenhuma mudança é necessária nas policies `[Authorize(Roles = "...")]` já existentes.
- O segredo JWT passa a ser compartilhado entre dois repositórios (app e lambda) via SSM Parameter Store (`SecureString`) — ver [ADR 0005](../adr/0005-jwt-simetrico-compartilhado.md) e [ADR 0008](../adr/0008-prioridade-de-custo-aws-academy.md).
- `Usuario` ganha `Cpf` (único, obrigatório, validado com o mesmo algoritmo de dígito verificador usado por `Cliente.Documento`) e `Ativo` (default `true`) — ver [DER](../der.md).
- Nenhuma rota de autoatendimento para `Cliente` foi adicionada — está fora do escopo desta fase; `Cliente.Ativo` existe como propriedade de cadastro, sem uso em autenticação.
