# Tech Challenge - FIAP

>Este projeto consiste no desenvolvimento do MVP do back-end de um Sistema Integrado de Atendimento e Execução de Serviços para uma oficina mecânica.
O sistema permite o gerenciamento de clientes, veículos, produtos, serviços e ordens de serviço.

---

## Configuração do ambiente

Adicione o arquivo `.env` e configure conforme `.env.example`:

- `POSTGRES_PASSWORD`: defina a senha do banco de dados  
- `ConnectionStrings__Default`: ajuste a senha para corresponder à definida no banco  
- `JwtSettings__Secret`: utilize uma chave forte (mínimo de 32 caracteres)  

> A conexão com o banco já está configurada para o PostgreSQL local que será iniciado via Docker.

## Execução da aplicação

A aplicação pode ser executada de duas formas:

- **Docker**: utilizando `docker compose up -d`  
- **Localmente**: rodando diretamente pela IDE ou CLI (necessário ter o .NET e o PostgreSQL configurados)

---

## Tech Stack

O projeto foi desenvolvido utilizando **C# com .NET 9** e **PostgreSQL 16** como banco de dados principal.

A escolha do PostgreSQL foi feita considerando:

- Consistência transacional — suporte completo a ACID  
- Concorrência avançada — controle de concorrência multiversão (MVCC)  
- Modelagem de domínio — recursos nativos que facilitam regras complexas de negócio  
- Maturidade e confiabilidade — amplamente utilizado em produção no mercado  
