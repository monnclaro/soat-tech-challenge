# Infraestrutura — SOAT Tech Challenge

## Kubernetes

A infraestrutura é orquestrada no Kubernetes (Minikube) dentro do namespace `soat`.

<img src="images/kubernetes.svg" alt="Diagrama Kubernetes" width="680"/>

### Recursos provisionados

| Recurso | Nome | Tipo | Descrição |
|---|---|---|---|
| Namespace | `soat` | Namespace | Isolamento de todos os recursos |
| Secret | `postgres-secret` | Secret | Credenciais do banco |
| PVC | `postgres-pvc` | PersistentVolumeClaim | Disco de 1Gi para os dados |
| StatefulSet | `postgres` | StatefulSet | PostgreSQL 16-alpine |
| Service | `postgres-service` | ClusterIP | Acesso interno ao banco :5432 |
| ConfigMap | `soat-api-config` | ConfigMap | Variáveis de ambiente da API |
| Secret | `soat-api-secret` | Secret | Segredos da API |
| Deployment | `soat-api` | Deployment | API com rolling update |
| Service | `soat-api-service` | NodePort | Acesso externo à API :80 |
| HPA | `soat-api-hpa` | HorizontalPodAutoscaler | Escalabilidade automática |

---

## Fluxo de Deploy (CI/CD)

O pipeline é acionado automaticamente a cada push na branch `main` via **GitHub Actions com self-hosted runner** (Windows + Minikube).

<img src="images/cicd.svg" alt="Fluxo CI/CD" width="680"/>

### Etapas do pipeline

| Etapa | Descrição |
|---|---|
| **Checkout** | `actions/checkout@v4.2.2` |
| **Minikube** | `minikube status` → `minikube start --driver=docker --wait=all` automaticamente se o cluster não estiver de pé; configura `KUBECONFIG` e troca o contexto do `kubectl` para `minikube` |
| **Setup .NET 9** | `actions/setup-dotnet@v4.3.1` |
| **Restaurar dependências** | `dotnet restore` |
| **Build** | `dotnet build --no-restore --configuration Release` |
| **Testes** | `dotnet test --no-build --configuration Release` |
| **Docker build** | `docker build -t soat-api:latest -f src/Api/Dockerfile .` |
| **Carregar imagem** | `minikube image load soat-api:latest` |
| **Terraform version** | `terraform version` (verifica que o binário já está disponível no runner) |
| **Terraform Init** | `terraform init -input=false` em `infra/` |
| **Terraform Format Check** | `terraform fmt -check` |
| **Terraform Validate** | `terraform validate` |
| **Terraform Apply** | `terraform apply -auto-approve -input=false -var="restart_trigger=<run_id>"` — o `run_id` do GitHub Actions força o rollout do Deployment a cada execução |
| **Verificação** | `kubectl get all -n soat` |

---

## Terraform

O Terraform provisiona toda a infraestrutura Kubernetes como código.

### Estrutura dos módulos

```
infra/
├── main.tf           ← provider + namespace + chamada dos módulos
├── variables.tf      ← db_name, db_user, db_password, jwt_secret
├── outputs.tf        ← como acessar a aplicação
└── modules/
    ├── postgres/     ← Secret + PVC + StatefulSet + Service
    └── app/          ← ConfigMap + Secret + Deployment + Service + HPA
```

### Executar

Em CI, essas etapas rodam automaticamente a cada push em `main` (ver tabela acima). Para rodar manualmente em um ambiente local:

```powershell
cd infra
terraform init
terraform apply
```