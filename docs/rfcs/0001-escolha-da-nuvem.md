# RFC 0001 — Escolha da Nuvem

**Status:** Aceito
**Autores:** Equipe SOAT Tech Challenge
**Contexto:** Fase 3 exige API Gateway, Function Serverless, banco gerenciado e cluster Kubernetes escalável, com "livre escolha de nuvem".

## Problema

Precisamos escolher um provedor de nuvem para hospedar API Gateway, função serverless de autenticação, banco de dados gerenciado e cluster Kubernetes, com o menor atrito possível de migração a partir do estado atual (Fase 2: Docker + Kubernetes + Terraform + GitHub Actions).

## Alternativas consideradas

| Critério | AWS | Azure | GCP |
|---|---|---|---|
| API Gateway | API Gateway (nome literal do enunciado) | API Management | API Gateway |
| Serverless | Lambda | Functions | Cloud Functions |
| Kubernetes gerenciado | EKS | AKS | GKE |
| Banco gerenciado | RDS | Azure Database | Cloud SQL |
| Terraform provider | Mais maduro/documentado (`terraform-aws-modules`) | Maduro | Maduro |
| Familiaridade da equipe | Já usada em outros trabalhos do grupo | Nenhuma | Nenhuma |
| Free tier para o projeto acadêmico | RDS/EKS free tier limitado, mas suficiente para o exercício | Similar | Similar |

## Decisão

**AWS.** Além da aderência literal aos nomes de serviço citados no enunciado (API Gateway, Lambda), o ecossistema Terraform para AWS (`terraform-aws-modules/vpc`, `terraform-aws-modules/eks`) é o mais maduro entre os três, reduzindo o código de infraestrutura escrito à mão e o risco de erro em um projeto com prazo fixo.

## Consequências

- Runtime do Lambda fica restrito ao que a AWS publica como managed runtime — impactou diretamente a escolha de versão do .NET no Lambda (ver [ADR 0006](../adr/0006-lambda-dotnet8-independente-do-app.md)).
- Toda a IAM, VPC e nomenclatura de recursos nos 3 repositórios de infraestrutura é AWS-específica; portar para outra nuvem exigiria reescrever `infra-k8s`, `infra-database` e a metade do `lambda` que é Terraform.
- Autenticação dos pipelines de CI/CD com a AWS: a conta usada é um **AWS Academy Learner Lab**, que não suporta OIDC nem IAM roles de longa duração — os workflows usam as credenciais de sessão temporária do próprio Lab. Detalhes e todo o recorte de custo decorrente: [ADR 0008](../adr/0008-prioridade-de-custo-aws-academy.md).
