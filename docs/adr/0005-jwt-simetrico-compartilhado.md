# ADR 0005 — JWT Simétrico (HS256) Compartilhado, Não Assimétrico (RS256/JWKS)

**Status:** Aceito

## Contexto

O JWT emitido pelo login por CPF (Lambda `AuthFunction`) precisa ser validado pela API principal (`AddJwtAuthentication`, já existente desde a Fase 1) — a única validação que existe, já que não há Lambda Authorizer no API Gateway (ver [ADR 0009](./0009-sem-lambda-authorizer.md)). Ainda assim, **dois repositórios diferentes** (o `lambda`, que emite o token, e o `soat-tech-challenge`, que valida) precisam concordar sobre o segredo usado.

## Decisão

Manter HS256 (segredo simétrico compartilhado), como já era feito para os tokens de `Usuario` desde a Fase 1. O segredo é gerado pelo Terraform do repositório `infra-k8s` (`random_password.jwt_secret`, em `jwt.tf`) — **não** pelo `lambda`, apesar de ser ele quem emite o token via CPF: o bootstrap dos 4 repositórios é uma cadeia (infra-k8s → infra-database → app → lambda), e tanto app quanto lambda leem esse segredo do SSM. Se o `lambda` fosse o dono, o primeiro deploy do app falharia (segredo ainda não existe, pois lambda só aplica depois do app) — e o `lambda` também não conseguiria aplicar antes do app (precisa do IP do node, publicado pelo app). `infra-k8s`, por ser o primeiro da cadeia e não depender de mais ninguém, é o único lugar onde isso não gera uma dependência circular. O segredo fica guardado como SSM Parameter `SecureString` (não Secrets Manager — ver [ADR 0008](./0008-prioridade-de-custo-aws-academy.md)), e é lido por ambos os consumidores como variável de ambiente: a Lambda via `LambdaConfig.JwtSecret` (lido de um `data "aws_ssm_parameter"`); a API via Secret do Kubernetes, populado em tempo de deploy a partir do mesmo parâmetro SSM.

## Alternativas descartadas

- **RS256 com JWKS**: o emissor assina com chave privada, e qualquer validador busca a chave pública num endpoint `/.well-known/jwks.json` — é o padrão mais robusto para múltiplos consumidores e emissores desacoplados, e seria nativamente suportado pelo *JWT Authorizer* embutido do API Gateway (se este repositório usasse um). Foi descartado nesta fase porque exigiria: (1) expor um endpoint JWKS publicamente a partir de algum lugar (o app? o Lambda?), (2) reescrever `JwtTokenProvider`/`AddJwtAuthentication` do app (hoje simétrico) e (3) gerenciar rotação de chave assimétrica — trabalho desproporcional ao valor para o tamanho atual do sistema, ainda mais sem authorizer no Gateway pra se beneficiar do suporte nativo.
- **Segredos diferentes por caminho de login** (um para email/senha, outro para CPF): obrigaria a API a saber, antes de validar, qual segredo tentar — complexidade sem benefício de segurança real, já que os dois caminhos emitem token para a mesma identidade (`Usuario`) e são ambos sistemas internos confiáveis (não há emissor de terceiros).

## Consequências

- Um único segredo comprometido invalida a confiança em **todos** os tokens de `Usuario`, emitidos por qualquer um dos dois caminhos de login — superfície de risco maior que RS256, onde vazar a chave pública não compromete nada.
- Rotação do segredo exige coordenar o redeploy de três repositórios (`infra-k8s`, que o gera, e `app`/`lambda`, que o consomem) — não há automação de rotação nesta fase.
- Caminho de evolução natural, se o sistema crescer para múltiplos consumidores externos de API: migrar para RS256/JWKS.
