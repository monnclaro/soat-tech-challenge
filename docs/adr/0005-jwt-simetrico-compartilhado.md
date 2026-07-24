# ADR 0005 — JWT Simétrico (HS256) Compartilhado, Não Assimétrico (RS256/JWKS)

**Status:** Aceito

## Contexto

O JWT emitido para clientes (Lambda `AuthFunction`) precisa ser validado tanto pela API principal (`AddJwtAuthentication`, já existente desde a Fase 1) quanto, opcionalmente, pelo Lambda Authorizer no API Gateway. Isso significa que **dois runtimes diferentes** (ASP.NET Core no EKS e uma função Lambda) precisam concordar sobre a validade de um token.

## Decisão

Manter HS256 (segredo simétrico compartilhado), como já era feito para os tokens de `Usuario` desde a Fase 1. O segredo é gerado pelo Terraform do repositório `lambda` (`random_password.jwt_secret`), guardado como SSM Parameter `SecureString` (não Secrets Manager — ver [ADR 0008](./0008-prioridade-de-custo-aws-academy.md)), e lido por ambos os consumidores como variável de ambiente: API via Secret do Kubernetes populado em tempo de deploy a partir do SSM; Lambda Authorizer via `LambdaConfig.JwtSecret`, injetado pelo Terraform diretamente na função.

## Alternativas descartadas

- **RS256 com JWKS**: o emissor assina com chave privada, e qualquer validador busca a chave pública num endpoint `/.well-known/jwks.json` — é o padrão mais robusto para múltiplos consumidores e emissores desacoplados, e é nativamente suportado pelo *JWT Authorizer* embutido do API Gateway (sem precisar de uma Lambda Authorizer customizada). Foi descartado nesta fase porque exigiria: (1) expor um endpoint JWKS publicamente a partir de algum lugar (o app? o Lambda?), (2) reescrever `JwtTokenProvider`/`AddJwtAuthentication` do app (hoje simétrico) e (3) gerenciar rotação de chave assimétrica — trabalho desproporcional ao valor para o tamanho atual do sistema.
- **Segredos diferentes por emissor** (um para `Usuario`, outro para `Cliente`): obrigaria a API a saber, antes de validar, qual segredo tentar — complexidade sem benefício de segurança real, já que ambos os emissores são sistemas internos confiáveis (não há emissor de terceiros).

## Consequências

- Um único segredo comprometido invalida a confiança em **todos** os tokens (funcionário e cliente) — superfície de risco maior que RS256, onde vazar a chave pública não compromete nada.
- Rotação do segredo exige coordenar o redeploy de dois repositórios (`lambda`, que o gera, e `app`, que o consome via Secret) — não há automação de rotação nesta fase.
- Caminho de evolução natural, se o sistema crescer para múltiplos consumidores externos de API: migrar para RS256/JWKS e usar o *JWT Authorizer* nativo do API Gateway no lugar do Lambda Authorizer customizado.
