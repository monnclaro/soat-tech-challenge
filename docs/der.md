# Diagrama Entidade-Relacionamento

Justificativa da escolha do banco: [rfcs/0002-escolha-do-banco-de-dados.md](./rfcs/0002-escolha-do-banco-de-dados.md).

```mermaid
erDiagram
    CLIENTE ||--o{ VEICULO : possui
    CLIENTE ||--o{ ORDEM_SERVICO : solicita
    VEICULO ||--o{ ORDEM_SERVICO : "é atendido em"
    ORDEM_SERVICO ||--o{ ORDEM_SERVICO_PRODUTO : contem
    ORDEM_SERVICO ||--o{ ORDEM_SERVICO_SERVICO : contem
    PRODUTO ||--o{ ORDEM_SERVICO_PRODUTO : "referenciado em"
    SERVICO ||--o{ ORDEM_SERVICO_SERVICO : "referenciado em"
    USUARIO ||--o{ USUARIO_ROLE : possui

    CLIENTE {
        uuid id PK
        string nome
        string documento UK "CPF ou CNPJ, único"
        enum tipo_documento "Cpf | Cnpj"
        boolean ativo "novo na Fase 3 — propriedade de cadastro, sem uso em autenticação"
        datetime data_criacao
    }

    VEICULO {
        uuid id PK
        uuid id_cliente FK
        string placa UK
        string marca
        string modelo
        int ano
        datetime data_criacao
    }

    USUARIO {
        uuid id PK
        string nome
        string email UK
        string senha_hash
        string cpf UK "novo na Fase 3 — checado pelo Lambda de auth"
        boolean ativo "novo na Fase 3 — checado pelo Lambda de auth"
        datetime data_criacao
    }

    USUARIO_ROLE {
        uuid id PK
        uuid id_usuario FK
        string role "ex.: Admin"
    }

    ORDEM_SERVICO {
        uuid id PK
        uuid id_cliente FK
        uuid id_veiculo FK
        enum status "Recebida..Entregue"
        decimal valor_total
        datetime data_criacao
        datetime data_inicio_execucao "nullable"
        datetime data_finalizacao "nullable"
    }

    ORDEM_SERVICO_PRODUTO {
        uuid id PK
        uuid id_ordem_servico FK
        uuid id_produto FK
        string nome_produto "snapshot no momento da inclusão"
        decimal valor_unitario "snapshot"
        decimal quantidade
    }

    ORDEM_SERVICO_SERVICO {
        uuid id PK
        uuid id_ordem_servico FK
        uuid id_servico FK
        string nome_servico "snapshot"
        decimal valor "snapshot"
        enum status
        datetime data_inicio_execucao "nullable"
        datetime data_finalizacao_execucao "nullable"
    }

    PRODUTO {
        uuid id PK
        string nome
        string descricao
        decimal valor
        decimal quantidade_em_estoque
    }

    SERVICO {
        uuid id PK
        string nome
        string descricao
        decimal valor
    }
```

## Explicação dos relacionamentos

- **Cliente 1—N Veículo**: um cliente pode ter vários veículos cadastrados (`Veiculo.IdCliente`).
- **Cliente 1—N OrdemServico** e **Veiculo 1—N OrdemServico**: cada OS pertence a exatamente um cliente e um veículo — a FK dupla existe porque, embora o veículo já implique o cliente, a consulta por cliente (`BuscarPaginadoPorDocumento`) e por veículo são ambas caminhos de acesso frequentes o suficiente para justificar guardar as duas FKs em vez de fazer join através de `Veiculo` sempre.
- **OrdemServico 1—N OrdemServicoProduto/OrdemServicoServico**: tabelas de associação que também guardam um **snapshot** de nome e valor no momento da inclusão (`NomeProduto`, `ValorUnitario`, `NomeServico`, `Valor`) — decisão de modelagem anterior à Fase 3, mantida porque altera a rastreabilidade histórica: se o preço de um produto mudar depois, ordens de serviço antigas continuam refletindo o valor cobrado na época, não o valor atual do catálogo.
- **Produto/Servico 1—N (via tabelas de associação)**: o catálogo (`Produto`, `Servico`) é referenciado, mas nunca alterado, por uma OS já criada — mudanças em `Produto.Valor`/`Servico.Valor` não retroagem.
- **Usuario 1—N UsuarioRole**: um funcionário pode ter múltiplas roles (hoje, na prática, só `Admin` é usada nos controllers).
- **Usuario.Cpf / Usuario.Ativo** (Fase 3): não são relacionamentos, mas o principal ajuste desta fase no modelo — necessários para o Lambda de autenticação decidir, a partir do CPF informado, se aquele `Usuario` existe e está ativo antes de emitir um token (segunda forma de login, ver [RFC 0003](./rfcs/0003-estrategia-de-autenticacao.md)).
- **Cliente.Ativo** (Fase 3): adicionada por completude de cadastro na mesma entrega, mas não participa de autenticação — `Cliente` não loga no sistema.

## O que mudou nesta fase

Ver [RFC 0002](./rfcs/0002-escolha-do-banco-de-dados.md) para a justificativa completa. Resumo: banco migrou de StatefulSet em Kubernetes para Amazon RDS ([ADR 0007](./adr/0007-postgres-gerenciado.md)); schema ganhou `Cliente.Ativo` e `Usuario.Cpf`/`Usuario.Ativo`.
