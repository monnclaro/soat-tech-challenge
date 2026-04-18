# Tech Challenge - FIAP

Este projeto consiste no desenvolvimento do MVP do back-end de um Sistema Integrado de Atendimento e Execução de Serviços para uma oficina mecânica.
O sistema permite o gerenciamento de clientes, veículos, produtos, serviços e ordens de serviço.


## Instalação e Execução
  
Não é necessário instalar manualmente a aplicação nem o banco de dados. Todo o ambiente é provisionado automaticamente pelos containers.
 > **Pré-requisito único:** [Docker](https://www.docker.com/get-started) instalado na máquina.

### 1. Clone o repositório
 
```bash
git clone https://github.com/monnclaro/soat-tech-challenge.git
cd soat-tech-challenge
```
 
### 2. Suba os containers
 
```bash
docker compose up -d
```
 
### 3. Acesse a aplicação
 
Aguarde os containers inicializarem e acesse a **interface interativa da API**:
 
```
http://localhost:8080/scalar/v1
```
 
> A interface do Scalar permite explorar e testar todos os endpoints diretamente pelo navegador, sem necessidade de ferramentas externas.

 
## Banco de Dados
 
O projeto utiliza **PostgreSQL 16** como banco de dados principal.
 
A escolha foi feita considerando:
 
-  **Consistência transacional** — suporte completo a ACID;
-  **Concorrência avançada** — controle de concorrência multiversão (MVCC);
-  **Modelagem de domínio** — recursos nativos que facilitam regras complexas de negócio;
-  **Maturidade e confiabilidade** — amplamente utilizado em produção no mercado. 

