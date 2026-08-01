# ADR 0006 — Lambda em .NET 8 (LTS), Independente da Versão do App

**Status:** Aceito

## Contexto

O app principal roda em .NET 9. Ao criar o repositório `lambda`, a primeira tentativa foi manter .NET 9 por consistência entre repositórios.

## Decisão

O Lambda roda em **.NET 8**. Confirmado via `terraform validate` (não apenas documentação): o schema do provider `hashicorp/aws` para `aws_lambda_function.runtime` não aceita `"dotnet9"` — os valores válidos param em `"dotnet8"`. A AWS só publica managed runtime para versões LTS do .NET; .NET 9 é STS (Standard Term Support, ~18 meses) e não tem managed runtime dedicado no momento desta implementação.

## Alternativas descartadas

- **Custom runtime self-contained (`provided.al2023`)**: mantém o código em net9.0, publicado como executável self-contained via `Amazon.Lambda.RuntimeSupport`. Viável, mas adiciona um empacotamento não padrão (bootstrap executável) ao pipeline de CI/CD de um repositório que já tem complexidade própria (duas funções + API Gateway).
- **Imagem de container (`package_type = "Image"`)**: suporta qualquer versão do .NET via uma imagem Docker publicada num ECR. Também viável, mas adicionaria um ECR + Dockerfile + build de imagem só para este repositório, quando o padrão managed runtime + zip já resolve o problema com menos peças móveis.

Ambas as alternativas ficam registradas como caminho de migração caso a AWS demore a publicar managed runtime para futuras versões do .NET e o time queira acompanhar a versão mais recente.

## Consequências

- O código do Lambda (`CpfValidator`, `JwtService` etc.) é uma **cópia deliberada** da lógica equivalente no app, não uma referência de projeto compartilhado — os dois repositórios podem, em teoria, divergir de versão de .NET no futuro sem se bloquearem mutuamente.
- Se a regra de validação de CPF mudar, precisa mudar nos dois lugares (app e lambda) — um custo de manutenção aceito em troca de manter os repositórios como unidades de deploy verdadeiramente independentes (requisito da Fase 3).
