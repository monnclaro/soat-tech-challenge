# Tech Challenge — FIAP

> Sistema Integrado de Atendimento e Execução de Serviços para uma oficina mecânica. Permite o gerenciamento de clientes, veículos, produtos, serviços e ordens de serviço — com infraestrutura escalável, orquestração em Kubernetes e pipeline de CI/CD automatizado.

---

## Fase 1 — MVP do Sistema da Oficina

Nesta primeira etapa foi desenvolvido o MVP do sistema de gerenciamento da oficina mecânica, com foco em organização operacional, rastreabilidade dos serviços e centralização das informações.

Objetivos da fase:

- Gerenciar clientes e veículos
- Controlar produtos e peças utilizadas nos serviços
- Registrar ordens de serviço
- Acompanhar o status dos atendimentos
- Garantir persistência e integridade dos dados
- Aplicar boas práticas de arquitetura e qualidade de software

---

## Fase 2 — Infraestrutura, Escalabilidade e CI/CD

Após a implantação do sistema inicial, com o aumento da demanda e a expansão para novas unidades, surgiu a necessidade de evoluir a aplicação. Os objetivos desta fase são:

- Reduzir riscos operacionais por meio de infraestrutura escalável
- Automatizar o provisionamento e o deploy do ambiente
- Melhorar a qualidade e organização do código com **Clean Architecture**
- Preparar a aplicação para suportar grandes volumes de ordens de serviço em horários de pico com escalabilidade dinâmica

### O que foi adicionado nesta fase

| O que | Descrição |
|---|---|
| **Clean Architecture** | Reorganização do código em camadas Domain, Application, Infrastructure e API |
| **Kubernetes** | Orquestração com Deployments, Services, ConfigMaps, Secrets e HPA |
| **Terraform** | Provisionamento da infraestrutura como código |
| **CI/CD** | Pipeline automatizado com GitHub Actions e self-hosted runner |
| **PostgreSQL containerizado** | StatefulSet com persistência via PVC |

---

## Fase 3 — Operação Corporativa (AWS, Serverless, Observabilidade)

Com a expansão para múltiplas unidades, o sistema passou a rodar em nuvem real (AWS), com uma segunda forma de login (por CPF) desacoplada em uma função serverless e observabilidade de ponta a ponta. O projeto foi **dividido em 4 repositórios**, cada um com CI/CD próprio:

| Repositório | Responsabilidade |
|---|---|
| **soat-tech-challenge** (este) | API principal, executando em Kubernetes |
| [soat-tech-challenge-infra-k8s](https://github.com/monnclaro/soat-tech-challenge-infra-k8s) | Terraform: VPC + cluster EKS + New Relic (nível de cluster) |
| [soat-tech-challenge-infra-database](https://github.com/monnclaro/soat-tech-challenge-infra-database) | Terraform: RDS PostgreSQL gerenciado |
| [soat-tech-challenge-lambda](https://github.com/monnclaro/soat-tech-challenge-lambda) | Login por CPF do `Usuario` (Lambda) + API Gateway |

O que mudou nesta fase:

- **Postgres em Minikube → RDS gerenciado** (`k8s/postgres/` foi removido; o banco agora é provisionado pelo infra-database).
- **Terraform local (`infra/`) removido** — a infraestrutura de cluster passou para o repositório infra-k8s; este repositório só aplica os manifests da própria aplicação (Deployment/Service/HPA) contra o cluster já existente.
- **Login por CPF**: `Usuario` (funcionário) ganhou uma segunda forma de login, além de email/senha — informando o CPF via Lambda de autenticação, recebe o mesmo tipo de token (role `Admin`) já aceito nas rotas sensíveis — ver [soat-tech-challenge-lambda](https://github.com/monnclaro/soat-tech-challenge-lambda) e [RFC 0003](./docs/rfcs/0003-estrategia-de-autenticacao.md). O login por email/senha (`POST /api/auth/login`) continua funcionando sem mudanças.
- **Exposição via `Service type=NodePort` + API Gateway** — sem ALB, para minimizar custo no AWS Academy (ver [ADR 0008](./docs/adr/0008-prioridade-de-custo-aws-academy.md)).
- **New Relic**: APM via init container (`newrelic-dotnet-init`, sem alterar a imagem da aplicação) + logs estruturados em JSON (Serilog) correlacionados por `X-Correlation-Id`.
- **Health check** em `/health`, usado pelo readiness/liveness probe do Kubernetes.

Diagramas, ADRs e RFCs completos da Fase 3: [docs/](./docs).

---

## Tech Stack

O projeto foi desenvolvido com **C# / .NET 9** e **PostgreSQL 16** (Amazon RDS) como banco de dados principal.

| Componente | Tecnologia | Descrição |
|---|---|---|
| API | ASP.NET Core 9 | REST API principal da oficina |
| Banco de Dados | PostgreSQL 16 (Amazon RDS) | Persistência dos dados |
| Orquestração | Kubernetes (Amazon EKS) | Deploy e escalabilidade automática |
| Logging | Serilog (JSON estruturado) | Correlação de requisições via `X-Correlation-Id` |
| Observabilidade | New Relic (APM + Kubernetes integration) | Métricas, logs e dashboards |
| CI/CD | GitHub Actions | Build, testes, imagem no ECR e deploy no EKS |

---

## Arquitetura

O projeto segue os princípios da **Clean Architecture**, organizado em camadas com dependências apontando sempre para o centro:

```
src/
├── Domain/              ← entidades, interfaces, regras de negócio puras
├── Application/         ← casos de uso, DTOs, serviços de aplicação
├── Infrastructure/      ← repositórios, banco de dados, serviços externos
├── API/                 ← controllers, middlewares, configuração HTTP
└── SharedKernel/        ← tipos comuns, extensões e utilitários compartilhados
```

- **Domain** não depende de nada — contém as entidades e contratos
- **Application** depende apenas do Domain
- **Infrastructure** implementa as interfaces do Domain
- **API** orquestra tudo e expõe os endpoints REST
- **SharedKernel** é referenciado por todas as camadas — contém tipos base, extensões e utilitários sem dependência de negócio

<img src="docs/images/architecture.svg" alt="Diagrama Clean Architecture" width="680"/>

---

## Infraestrutura e CI/CD

O cluster Kubernetes (Amazon EKS) e o banco (Amazon RDS) são provisionados pelos repositórios [infra-k8s](https://github.com/monnclaro/soat-tech-challenge-infra-k8s) e [infra-database](https://github.com/monnclaro/soat-tech-challenge-infra-database), respectivamente. Este repositório só é responsável pela aplicação: a cada push em `main`, o CI/CD builda a imagem, publica no ECR e aplica os manifests (`k8s/`) contra o cluster já existente — sem passos manuais.

Para o detalhamento completo — diagramas, recursos provisionados e fluxo de deploy — veja [docs/infra.md](./docs/infra.md).

---

## Como começar

```bash
git clone https://github.com/monnclaro/soat-tech-challenge
cd soat-tech-challenge
```

---

## Execução local

### Com Docker

Copie o arquivo de exemplo e preencha as variáveis:

```bash
cp .env.example .env
```

| Variável | Descrição |
|---|---|
| `POSTGRES_PASSWORD` | Senha do banco de dados PostgreSQL |
| `ConnectionStrings__Default` | String de conexão (ajuste a senha conforme definida acima) |
| `JwtSettings__Secret` | Chave JWT (mínimo 32 caracteres) |
| `Webhook__Secret` | Secret Webhook |

### Sem Docker

Ajuste o arquivo `appsettings.Development.json` na raiz do projeto:

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=soattechchallenge;User Id=postgres;Password=sua_senha_aqui;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "Secret": "SuaChaveSuperSecretaComMinimo32Caracteres",
    "ExpirationHours": 2
  },
  "Webhook": {
    "Secret": "secret_webhook"
  }
}
```

---

## Deploy em Kubernetes (Amazon EKS)

Pré-requisito: o cluster já provisionado pelo [infra-k8s](https://github.com/monnclaro/soat-tech-challenge-infra-k8s) e o banco pelo [infra-database](https://github.com/monnclaro/soat-tech-challenge-infra-database) (ver ordem de deploy no README do repo lambda).

```bash
# 1. Apontar o kubectl para o cluster
aws eks update-kubeconfig --name soat-producao --region us-east-1

# 2. Build e push da imagem
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin <account-id>.dkr.ecr.us-east-1.amazonaws.com
docker build -t <account-id>.dkr.ecr.us-east-1.amazonaws.com/soat-api:local -f src/Api/Dockerfile .
docker push <account-id>.dkr.ecr.us-east-1.amazonaws.com/soat-api:local

# 3. Gerar o Secret a partir do exemplo (nunca commitar com valores reais) e aplicar os manifests
cp k8s/secret.example.yaml k8s/secret.local.yaml   # preencha os valores antes de aplicar
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secret.local.yaml
ECR_IMAGE=<account-id>.dkr.ecr.us-east-1.amazonaws.com/soat-api:local envsubst < k8s/deployment.yaml | kubectl apply -f -
kubectl apply -f k8s/service.yaml
kubectl apply -f k8s/hpa.yaml

# 4. Endereço público (NodePort — sem ALB, ver ADR 0008)
kubectl get nodes -o wide   # ExternalIP de qualquer node + porta 30080
```

> No CI/CD ([.github/workflows/ci-cd.yml](.github/workflows/ci-cd.yml)) esses passos rodam automaticamente a cada push em `main`: build da imagem, push no ECR, geração do Secret a partir do SSM Parameter Store (RDS + JWT) e `kubectl apply` — sem passos manuais.

---

## API

Sem ALB: a app é exposta via `Service type=NodePort` (porta `30080`) — endereço público em `kubectl get nodes -o wide` (coluna `EXTERNAL-IP`). Em produção, o tráfego chega via o API Gateway do repositório [lambda](https://github.com/monnclaro/soat-tech-challenge-lambda), que autentica e encaminha direto pro IP do node. Justificativa da troca de ALB por NodePort: [ADR 0008](./docs/adr/0008-prioridade-de-custo-aws-academy.md).

📄 Collection: [collection.json](./collection.json)
