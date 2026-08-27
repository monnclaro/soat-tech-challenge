# RFC 0003 — Estratégia de Autenticação

**Status:** Aceito
**Contexto:** Fase 3 exige autenticação por CPF via função serverless que valida o CPF, consulta existência/status do cliente e emite um JWT — confirmado pelo professor: "o mais importante é atender ao requisito de identificação do cliente [...] vocês podem utilizar e-mail e senha como credenciais de acesso, desde que o CPF do cliente também seja validado e associado corretamente ao usuário".

## Problema

`Usuario` (funcionário) já loga com email/senha (`POST /api/auth/login`), emitindo um JWT validado pela própria API — isso continua inalterado. O enunciado pede, **além** disso, login por CPF para o `Cliente` da oficina, servido por uma função serverless.

Cogitou-se, em versões anteriores desta decisão, tratar o CPF como uma segunda forma de login do próprio `Usuario` (lido "rotas sensíveis" como "só quem já é admin deveria ganhar essa porta de entrada"). Reconsiderado: "rotas sensíveis" não significa que o `Cliente` precisa de acesso administrativo — só que o conjunto de rotas que o token de `Cliente` abre precisa ser **restrito e correto**, não que a identidade autenticada precise ser `Usuario`. O texto do enunciado e do professor usa "cliente" de forma consistente, e a única leitura que dá função a essas rotas específicas do `Cliente` (ver Decisão) é o próprio `Cliente` logando.

## Alternativas consideradas

| Opção | Prós | Contras |
|---|---|---|
| **CPF do `Cliente` + Lambda emite JWT com role própria** (escolhida) | Aderente ao enunciado e à clarificação do professor; dá função real às rotas de autoatendimento do cliente (consultar próprias OS, aprovar/reprovar próprio orçamento) | CPF sozinho é uma identificação fraca (qualquer um com o CPF de outra pessoa "loga") — mitigado por escopo restrito de rotas |
| CPF do `Usuario` + Lambda emite JWT | Reaproveitaria o login de funcionário já existente | Não dá função a nenhuma rota nova; "rotas sensíveis" do back-office não fazem sentido pro cliente ganhar acesso via CPF |
| CPF + senha (segundo fator) | Autenticação mais forte que CPF isolado | Fora do escopo pedido; exige fluxo de cadastro de senha adicional que não existe hoje para `Cliente` |

## Decisão

`Cliente` ganha uma forma de login por CPF: informa o CPF em `POST /auth/login-cpf` (API Gateway → Lambda `AuthFunction`), que valida o formato/dígitos verificadores, consulta `Cliente` por `Documento` no RDS, checa `Ativo`, e devolve um JWT HS256 com claims `Name` (nome do cliente), `Role = "Cliente"`, `NameIdentifier` (Id do cliente) e uma claim customizada `documento` — os dois últimos existem especificamente para autorização por propriedade (ver abaixo), não fazem parte do formato de token do `Usuario`.

O token de `Cliente` só abre rotas explicitamente escopadas pra ele em `OrdemServicosController` (`[Authorize(Roles = "Cliente,Admin")]`, `Admin` sempre com acesso total):

- `GET /api/v1/ordens-servico/cliente?documento=...` — lista as próprias OS. Se o `documento` da query não bater com o `documento` do token, devolve resultado vazio (não erro) em vez de consultar outro cliente.
- `PATCH /api/v1/ordens-servico/{id}/orcamento/aprovacao` — aprova o próprio orçamento.
- `PATCH /api/v1/ordens-servico/{id}/orcamento/reprovacao` — reprova o próprio orçamento.

Nas duas últimas, o use case compara `NameIdentifier` do token com `OrdemServico.IdCliente` — se não bater, devolve `404` (não `403`, de propósito: não confirma pra um `Cliente` que uma OS de outro cliente existe). `Admin` nunca passa por essa checagem — mantém acesso irrestrito a todas as OS, igual antes.

Todas as demais rotas (`Inserir`, `InserirProdutos`, `IniciarDiagnostico`, `Remover`, etc.) continuam `Admin`-only, sem nenhuma mudança.

## Consequências

- Duas populações de token coexistem: `Usuario` (funcionário, email/senha, role `Admin`, emitido pela API) e `Cliente` (CPF, role `Cliente`, emitido pelo Lambda) — diferenciadas pela claim `Role`, com formatos de claims ligeiramente diferentes (`Cliente` carrega `NameIdentifier`/`documento` extras).
- `Usuario.Cpf`/`Usuario.Ativo` (adicionados numa iteração anterior desta decisão) permanecem no domínio mesmo sem uso em autenticação — não fazem mal parados, e evitam uma migration a mais só pra removê-los.
- O segredo JWT é compartilhado entre app e lambda via SSM Parameter Store (`SecureString`), gerado pelo `infra-k8s` — ver [ADR 0005](../adr/0005-jwt-simetrico-compartilhado.md) e [ADR 0008](../adr/0008-prioridade-de-custo-aws-academy.md).
- Autorização por propriedade (dono do recurso) é feita na camada de Application (use case), não no `[Authorize]` do controller — o controller só extrai a claim e passa pro Input; a decisão de "é dono ou não" fica com quem já carrega a entidade carregada do banco.
