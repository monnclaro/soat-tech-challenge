# Tech Challenge - FIAP
 
> Sistema Integrado de Atendimento e Execução de Serviços para uma oficina mecânica. Permite o gerenciamento de clientes, veículos, produtos, serviços e ordens de serviço — com infraestrutura escalável, orquestração em Kubernetes e pipeline de CI/CD automatizado.
 
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

## Componentes da Aplicação

| Componente | Tecnologia | Descrição |
|---|---|---|
| API | ASP.NET Core 9 | REST API principal da oficina |
| Banco de Dados | PostgreSQL 16 | Persistência dos dados |
| Orquestração | Kubernetes (Minikube) | Deploy e escalabilidade automática |
| IaC | Terraform | Provisionamento da infraestrutura como código |
| CI/CD | GitHub Actions | Automação de build, testes e deploy |

--- 

## Tech Stack
 
O projeto foi desenvolvido com **C# / .NET 9** e **PostgreSQL 16** como banco de dados principal.
 
A escolha do PostgreSQL foi feita considerando:
 
- **Consistência transacional** — suporte completo a ACID
- **Concorrência avançada** — controle de concorrência multiversão (MVCC)
- **Modelagem de domínio** — recursos nativos que facilitam regras complexas de negócio
- **Maturidade e confiabilidade** — amplamente utilizado em produção no mercado

---

## Arquitetura

O projeto segue os princípios da **Clean Architecture**, organizado em camadas com dependências apontando sempre para o centro:

```
SoatTechChallenge/
├── Domain/              ← entidades, interfaces, regras de negócio puras
├── Application/         ← casos de uso, DTOs, serviços de aplicação
├── Infrastructure/      ← repositórios, banco de dados, serviços externos
└── API/                 ← controllers, middlewares, configuração HTTP
```

- **Domain** não depende de nada — contém as entidades e contratos
- **Application** depende apenas do Domain
- **Infrastructure** implementa as interfaces do Domain
- **API** orquestra tudo e expõe os endpoints REST

Para o detalhamento da infraestrutura provisionada e fluxo de deploy, veja [docs/infra.md](./docs/infra.md).

---

## Como começar
 
```bash
git clone https://github.com/monnclaro/soat-tech-challenge
cd soat-tech-challenge
```

## Configuração do ambiente
 
### Docker
 
Copie o arquivo de exemplo e preencha as variáveis:
 
```bash
cp .env.example .env
```
 
Edite o `.env` com os valores adequados:
 
| Variável | Descrição |
|---|---|
| `POSTGRES_PASSWORD` | Senha do banco de dados PostgreSQL |
| `ConnectionStrings__Default` | String de conexão (ajuste a senha conforme definida acima) |
| `JwtSettings__Secret` | Chave JWT (mínimo 32 caracteres) |
| `Webhook__Secret` | Secret Webhook |

---
 
### Local (sem Docker)
 
Ajuste o arquivo `appsettings.Development.json` na raiz do projeto com o conteúdo abaixo e ajuste conforme seu ambiente:
 
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

## Kubernetes (Minikube)

```powershell
# 1. Iniciar o cluster
minikube start
minikube addons enable metrics-server

# 2. Buildar e carregar a imagem
docker build -t soat-api:latest -f SoatTechChallenge/Dockerfile .
minikube image load soat-api:latest

# 3. Aplicar os manifestos
kubectl apply -f k8s/postgres/
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secret.yaml
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml
kubectl apply -f k8s/hpa.yaml

# 4. Abrir no browser
minikube service soat-api-service -n soat
```

---

## Infraestrutura com Terraform

O Terraform provisiona toda a infraestrutura Kubernetes como código:

```powershell
cd infra
terraform init
terraform apply
```

Recursos criados: namespace, PostgreSQL (Secret + PVC + StatefulSet + Service), aplicação (ConfigMap + Secret + Deployment + Service + HPA).

---

## CI/CD

O pipeline roda automaticamente a cada push na branch `main` via **GitHub Actions com self-hosted runner** (Windows + Minikube):

1. Build da aplicação
2. Execução dos testes automatizados
3. Build da imagem Docker
4. Carregamento da imagem no Minikube
5. Deploy do banco de dados
6. Aplicação dos manifestos Kubernetes
7. Rolling update do Deployment

---

## API

A porta é exibida ao rodar `minikube service soat-api-service -n soat`.

📄 Collection Postman: [postman/collection.json](./postman/collection.json)