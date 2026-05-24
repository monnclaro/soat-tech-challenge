# Infraestrutura — SOAT Tech Challenge

## Kubernetes

```
┌─────────────────────────────────────────────────────────────┐
│                    Kubernetes (Minikube)                     │
│                      Namespace: soat                        │
│                                                             │
│  ┌──────────────────────┐     ┌───────────────────────┐    │
│  │  Deployment          │     │  StatefulSet           │    │
│  │  soat-api            │◄───►│  postgres              │    │
│  │  2–10 pods (HPA)     │     │  PostgreSQL 16         │    │
│  └──────────┬───────────┘     └──────────┬────────────┘    │
│             │                            │                  │
│  ┌──────────▼───────────┐     ┌──────────▼────────────┐    │
│  │  Service             │     │  Service               │    │
│  │  soat-api-service    │     │  postgres-service      │    │
│  │  NodePort :80        │     │  ClusterIP :5432       │    │
│  └──────────────────────┘     └───────────────────────┘    │
│                                                             │
│  ┌─────────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │  HPA            │  │  ConfigMap   │  │  Secret      │  │
│  │  CPU > 70%      │  │  soat-api    │  │  soat-api    │  │
│  │  Mem > 80%      │  │  -config     │  │  -secret     │  │
│  │  min:2 max:10   │  └──────────────┘  └──────────────┘  │
│  └─────────────────┘                                       │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  PVC · postgres-pvc · 1Gi · StorageClass standard    │  │
│  └──────────────────────────────────────────────────────┘  │
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
GitHub Actions detecta o push
      │
      ▼
Self-hosted Runner (Windows + Minikube)
      │
      ├─── Build e testes ──────────────────────────────┐
      │    dotnet restore                               │
      │    dotnet build --configuration Release         │
      │    dotnet test --configuration Release          │
      │                                                 │
      ├─── Docker build ────────────────────────────────┤
      │    docker build -t soat-api:latest              │
      │                                                 │
      ├─── Carregar no Minikube ────────────────────────┤
      │    minikube image load soat-api:latest          │
      │                                                 │
      ├─── Deploy do banco ─────────────────────────────┤
      │    kubectl apply -f k8s/postgres/               │
      │    kubectl rollout status statefulset/postgres  │
      │                                                 │
      ├─── Apply manifestos ────────────────────────────┤
      │    kubectl apply -f k8s/configmap.yaml          │
      │    kubectl apply -f k8s/secret.yaml             │
      │    kubectl apply -f k8s/deployment.yaml         │
      │    kubectl apply -f k8s/service.yaml            │
      │    kubectl apply -f k8s/hpa.yaml                │
      │                                                 │
      ├─── Rolling update ──────────────────────────────┤
      │    kubectl set image deployment/soat-api        │
      │                                                 │
      └─── Verificar rollout ───────────────────────────┘
           kubectl rollout status deployment/soat-api
                 │
                 ▼
           Deploy concluído ✓
```

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