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
| **Setup .NET 9** | `actions/setup-dotnet@v4.3.1` |
| **Build** | `dotnet build --no-restore --configuration Release` |
| **Testes** | `dotnet test --no-build --configuration Release` |
| **Docker build** | `docker build -t soat-api:latest -f src/Api/Dockerfile .` |
| **Reinício do cluster** | `minikube delete` + `minikube start --driver=docker` |
| **Carregar imagem** | `minikube image load soat-api:latest` |
| **Namespace** | `kubectl create namespace soat` (se não existir) |
| **Deploy banco** | `kubectl apply -f k8s/postgres/` + rollout status (timeout 120s) |
| **Manifestos** | configmap · secret · deployment · service · hpa |
| **Rollout** | `kubectl rollout status deployment/soat-api` (timeout 180s) |
| **Verificação** | `kubectl get pods -n soat` |

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

```powershell
cd infra
terraform init
terraform apply
```
