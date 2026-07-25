# ADR 0001 — Split em 4 Repositórios, Lambda e API Gateway no Mesmo Repositório

**Status:** Aceito

## Contexto

O enunciado exige exatamente 4 repositórios: Lambda, Infraestrutura Kubernetes (Terraform), Infraestrutura do Banco (Terraform) e Aplicação principal. O API Gateway não tem repositório próprio na lista — precisa morar em um dos quatro.

## Decisão

O código do API Gateway (Terraform: HTTP API, rotas, integrations) vive no repositório **lambda**, junto da função `AuthFunction` (única Lambda deste repositório — ver [ADR 0009](./0009-sem-lambda-authorizer.md)). Motivo: o Gateway é o *trigger* da Lambda — são recursos acoplados no ciclo de vida (o Gateway não existe sem a Lambda por trás dele) e no deploy (o mesmo pipeline builda a função e aplica o Gateway). Colocar o Gateway em um repositório separado criaria um 5º repositório, violando o requisito, ou forçaria o Gateway para dentro do `infra-k8s`/`infra-database`, que são conceitualmente sobre infraestrutura de cluster e banco, não sobre roteamento de API.

## Alternativas descartadas

- **5º repositório "api-gateway"**: violaria diretamente "organizar o projeto em quatro repositórios".
- **Gateway dentro do `infra-k8s`**: misturaria uma preocupação de roteamento HTTP com provisionamento de cluster; o `infra-k8s` também não tem nenhuma dependência natural do Gateway (o cluster existe e funciona sem ele).

## Consequências

- O repositório `lambda` tem duas responsabilidades (compute serverless + roteamento HTTP), mas ambas voltadas exclusivamente para autenticação e proxy — não há lógica de negócio da oficina ali.
- Mudanças no roteamento de rotas protegidas (`/api/*`) exigem alterar o repositório `lambda`, não o `soat-tech-challenge` — uma dependência cruzada que precisa ser lembrada ao adicionar novos grupos de rota sensíveis.
