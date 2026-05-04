# Tech Challenge - FIAP
 
> Este projeto consiste no desenvolvimento do MVP do back-end de um Sistema Integrado de Atendimento e Execução de Serviços para uma oficina mecânica. O sistema permite o gerenciamento de clientes, veículos, produtos, serviços e ordens de serviço.
 
--- 

## Tech Stack
 
O projeto foi desenvolvido com **C# / .NET 9** e **PostgreSQL 16** como banco de dados principal.
 
A escolha do PostgreSQL foi feita considerando:
 
- **Consistência transacional** — suporte completo a ACID
- **Concorrência avançada** — controle de concorrência multiversão (MVCC)
- **Modelagem de domínio** — recursos nativos que facilitam regras complexas de negócio
- **Maturidade e confiabilidade** — amplamente utilizado em produção no mercado
  
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
  }
}
```
