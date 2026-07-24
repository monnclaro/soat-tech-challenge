# CI/CD e Governança dos 4 Repositórios

Este documento centraliza a configuração de CI/CD que é comum aos 4 repositórios da Fase 3 ([app](https://github.com/monnclaro/soat-tech-challenge), [infra-k8s](https://github.com/monnclaro/soat-tech-challenge-infra-k8s), [infra-database](https://github.com/monnclaro/soat-tech-challenge-infra-database), [lambda](https://github.com/monnclaro/soat-tech-challenge-lambda)): branches, environments, proteção e segredos. A configuração de pipeline específica de cada repositório fica no README dele.

> Autenticação com a AWS e estratégia de ambiente foram desenhadas para **AWS Academy** — ver [ADR 0008](./adr/0008-prioridade-de-custo-aws-academy.md) para o racional completo.

## Estratégia de branches

Só existem duas branches em cada repositório — sem ambiente de homologação, para minimizar custo:

| Branch | Papel |
|---|---|
| `master` | Branch de trabalho. PR roda build/test (e `terraform plan` nos repos de infra). Nenhum deploy acontece aqui. |
| `producao` | Deploy automático a cada push, mas gated por aprovação manual do GitHub Environment `producao`. |

`master` e `producao` são protegidas em todos os 4 repositórios (configurado diretamente no GitHub): sem commit direto, merge só via Pull Request.

## Setup — GitHub Environments

Os workflows referenciam o environment `producao` (`environment: producao` no job de deploy/apply). Ele precisa existir no GitHub antes do primeiro deploy, exigindo revisão manual. Rodar para cada um dos 4 repositórios:

```bash
REPO=monnclaro/soat-tech-challenge   # repetir para infra-k8s, infra-database, lambda

gh api "repos/$REPO/environments/producao" --method PUT -f "wait_timer=0" \
  -F "reviewers[][type]=Team" -F "reviewers[][id]=<team-id>"
```

> Ajuste `reviewers` para o time ou usuário responsável por aprovar deploys em produção. Sem isso, o `apply`/deploy roda sem revisão humana.

## Autenticação com a AWS (AWS Academy)

O Academy fornece credenciais de **sessão temporária** (Access Key, Secret Key, Session Token) que expiram junto com o Lab (tipicamente poucas horas) e **não** suporta OIDC do GitHub Actions (exigiria criar um IAM Identity Provider, ação bloqueada pela política restrita de IAM do Academy). Por isso, os 4 repositórios usam credenciais estáticas de sessão como GitHub Secrets, em vez de assumir uma IAM role via OIDC:

1. No painel do Lab, abrir **AWS Details** → copiar `aws_access_key_id`, `aws_secret_access_key` e `aws_session_token`.
2. Atualizar os secrets do repositório (`gh secret set AWS_ACCESS_KEY_ID`, etc., ou pela UI do GitHub) — **repetir isso a cada nova sessão do Lab**, já que as credenciais anteriores param de funcionar quando a sessão expira.

Pelo mesmo motivo (IAM restrito), cluster EKS, node group e as duas Lambdas não têm role própria — todos reaproveitam a `LabRole` já existente na conta, passada via `AWS_LAB_ROLE_ARN`.

## Segredos necessários por repositório

| Secret | app | infra-k8s | infra-database | lambda |
|---|:---:|:---:|:---:|:---:|
| `AWS_ACCESS_KEY_ID` | ✔ | ✔ | ✔ | ✔ |
| `AWS_SECRET_ACCESS_KEY` | ✔ | ✔ | ✔ | ✔ |
| `AWS_SESSION_TOKEN` | ✔ | ✔ | ✔ | ✔ |
| `AWS_LAB_ROLE_ARN` | | ✔ | | ✔ |
| `NEW_RELIC_LICENSE_KEY` | ✔ | ✔ | | |
| `NEW_RELIC_ACCOUNT_ID` | | ✔ | | |
| `NEW_RELIC_API_KEY` (dashboards/alertas) | | ✔ | | |
| `NEW_RELIC_NOTIFICATION_EMAIL` | | ✔ | | |
| `WEBHOOK_SECRET` | ✔ | | | |

`infra-database` não precisa de `AWS_LAB_ROLE_ARN` porque RDS não usa uma IAM role própria (é um recurso gerenciado pela AWS, sem role de execução como EKS/Lambda).

## Ordem de bootstrap de um ambiente novo

Ver detalhado em [soat-tech-challenge-lambda/README.md § Ordem de deploy](https://github.com/monnclaro/soat-tech-challenge-lambda#ordem-de-deploy-entre-os-4-reposit%C3%B3rios): infra-k8s → infra-database → app → lambda. Depois do bootstrap inicial, cada repositório aplica de forma independente — exceto que, sem ALB (ver ADR 0008), um deploy da app que troque o node ativo do EKS pode exigir reaplicar o `lambda` para atualizar o IP de destino do API Gateway.
