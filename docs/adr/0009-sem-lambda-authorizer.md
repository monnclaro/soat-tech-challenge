# ADR 0009 — Sem Lambda Authorizer no API Gateway

**Status:** Aceito
**Supersede:** a primeira versão desta arquitetura, que incluía uma segunda função (`AuthorizerFunction`) plugada como Lambda Authorizer nas rotas `/api/*`.

## Contexto

O enunciado pede uma única Function Serverless com três responsabilidades: validar o CPF, consultar existência/status na base e gerar/devolver um JWT. A primeira versão desta arquitetura implementou isso (`AuthFunction`) e **adicionou uma segunda Lambda** (`AuthorizerFunction`) como Lambda Authorizer do API Gateway, validando o JWT nas rotas `/api/*` antes de encaminhar pro node do EKS.

Essa segunda função não era exigida pelo enunciado — foi uma decisão de arquitetura adicional (defesa em profundidade: validar o token também na borda, não só na aplicação). Na prática, ela duplicava uma validação que a API principal **já fazia** desde a Fase 1 (`AddJwtAuthentication` + `[Authorize(Roles = ...)]`, mesmo segredo HS256).

## Decisão

Remover o `AuthorizerFunction`. O repositório `lambda` passa a ter uma única função (`AuthFunction`), exatamente como descrito no enunciado. A rota `ANY /api/{proxy+}` do API Gateway vira um `HTTP_PROXY` simples, sem `authorizer_id`/`authorization_type = "CUSTOM"` — a validação do Bearer JWT continua acontecendo, só que inteiramente dentro da API principal (que já fazia isso e não pode ser removida sem quebrar a proteção das rotas).

## Alternativas descartadas

- **Manter as duas Lambdas** (validação na borda + na aplicação): mais "defesa em profundidade", mas adiciona uma Lambda, uma configuração de Authorizer no API Gateway e um segundo lugar pra manter a lógica de validação de JWT sincronizada — custo de manutenção sem contrapartida em segurança real, já que ambas validam exatamente o mesmo token com o mesmo segredo.

## Consequências

- Repositório `lambda` mais simples: uma função, um `.csproj`, um `.zip` no CI/CD.
- A proteção das rotas `/api/*` depende inteiramente da API principal validar o JWT corretamente — se `AddJwtAuthentication`/`[Authorize]` forem removidos ou mal configurados na app, não há uma segunda camada no Gateway pra compensar. Aceito porque essa validação já existe e é testada desde a Fase 1, não é código novo introduzido por esta fase.
- Menos um componente na superfície de custo/manutenção (uma Lambda a menos, sem authorizer no API Gateway).
