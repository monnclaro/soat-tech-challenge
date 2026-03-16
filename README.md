# Tech Challenge - FIAP

Este projeto consiste no desenvolvimento do MVP do back-end de um Sistema Integrado de Atendimento e Execução de Serviços para uma oficina mecânica.
O sistema permite o gerenciamento de clientes, veículos, peças, serviços e ordens de serviço.

# Instalação e Execução do Projeto

Este projeto foi desenvolvido para ser executado utilizando **Docker**, portanto **não é necessário instalar manualmente a aplicação nem o banco de dados** na sua máquina. Todo o ambiente necessário é provisionado automaticamente pelos containers.

A API possui uma interface interativa para exploração e testes dos endpoints.

Após subir o projeto, acesse no navegador:

http://localhost:8080/scalar/v1

## Banco de Dados

O projeto utiliza PostgreSQL 16 como banco de dados principal.

A escolha do PostgreSQL foi feita considerando consistência transacional, suporte avançado a concorrência e recursos nativos para modelagem de domínio, que facilitam a implementação de regras complexas de negócio.
