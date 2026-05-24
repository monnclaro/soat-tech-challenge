# Infraestrutura — SOAT Tech Challenge

## Kubernetes

```
┌─────────────────────────────────────────────────────────────┐
│                    Kubernetes (Minikube)                     │
│                      Namespace: soat                        │
│                                                             │
│  ┌─────────────────────┐     ┌─────────────────────────┐   │
│  │     soat-api        │     │       postgres           │   │
│  │   Deployment        │◄───►│      StatefulSet         │   │
│  │   2–10 pods (HPA)   │     │      PVC 1Gi             │   │
│  └──────────┬──────────┘     └──────────┬──────────────┘   │
│             │                           │                   │
│  ┌──────────▼──────────┐     ┌──────────▼──────────────┐   │
│  │  soat-api-service   │     │   postgres-service       │   │
│  │  NodePort :80       │     │   ClusterIP :5432        │   │
│  └─────────────────────┘     └─────────────────────────┘   │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  soat-api-config (ConfigMap) + soat-api-secret       │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  soat-api-hpa — CPU > 70% ou Memória > 80%           │   │
│  │  min: 2 réplicas / max: 10 réplicas                  │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

### Recursos

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

```
git push → main
      │
      ▼
GitHub Actions
      │
      ▼
Self-hosted Runner (Windows + Minikube)
      │
      ├── 1. dotnet restore
      ├── 2. dotnet build --configuration Release
      ├── 3. dotnet test --configuration Release
      ├── 4. docker build -t soat-api:latest
      ├── 5. minikube image load soat-api:latest
      ├── 6. kubectl apply -f k8s/postgres/
      ├── 7. kubectl rollout status statefulset/postgres
      ├── 8. kubectl apply -f k8s/*.yaml
      ├── 9. kubectl set image deployment/soat-api
      └── 10. kubectl rollout status deployment/soat-api
```

### Por que Self-hosted Runner?

O runner roda na mesma máquina que o Minikube, eliminando a necessidade de expor o cluster na internet ou usar um registry externo. A imagem é buildada e carregada diretamente via `minikube image load`.

---

## Terraform — Módulos

```
infra/
├── main.tf           ← provider + namespace + chamada dos módulos
├── variables.tf      ← db_name, db_user, db_password, jwt_secret
├── outputs.tf        ← como acessar a aplicação
└── modules/
    ├── postgres/     ← Secret + PVC + StatefulSet + Service
    └── app/          ← ConfigMap + Secret + Deployment + Service + HPA
```