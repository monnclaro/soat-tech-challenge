# ADR 0003 — VPC Única, Criada pelo infra-k8s; RDS Liberado por CIDR

**Status:** Aceito

## Contexto

Três repositórios independentes (`infra-k8s`, `infra-database`, `lambda`) precisam de recursos na mesma rede AWS: o cluster EKS, o RDS e as ENIs do Lambda (quando configurado em VPC) precisam se enxergar. Cada repositório tem seu próprio state Terraform — não há um módulo compartilhado.

## Decisão

1. **`infra-k8s` é o único dono da VPC** (cria via `terraform-aws-modules/vpc`). Os demais repositórios apenas leem `vpc-id`, `private-subnet-ids` e `vpc-cidr` publicados em **SSM Parameter Store** (`/soat/{env}/network/*`) — nunca lêem o tfstate do infra-k8s diretamente.
2. **O Security Group do RDS libera a porta 5432 por CIDR da VPC**, não por referência a um Security Group específico (ex.: o SG dos nodes do EKS). Isso evita uma dependência circular de apply: se o RDS SG referenciasse o SG do Lambda, o `infra-database` precisaria que o `lambda` já tivesse sido aplicado (e vice-versa, se fosse ao contrário).

## Alternativas descartadas

- **Cada repo cria sua própria VPC com peering**: adiciona complexidade de rede (peering, rotas) sem necessidade — os 3 repositórios sempre operam como uma unidade lógica por ambiente.
- **Leitura direta do tfstate via `terraform_remote_state`**: acopla os repositórios à estrutura interna do state um do outro (troca de nome de recurso no infra-k8s quebraria o infra-database) e exigiria dar permissão de leitura do bucket S3 de state entre pipelines de repositórios diferentes. SSM Parameter Store expõe um contrato explícito e versionável (nome do parâmetro), não a estrutura interna do Terraform.
- **RDS SG referenciando o SG do EKS**: funcionaria para o EKS sozinho, mas quebraria assim que o Lambda (em outro repositório, aplicado de forma independente) também precisasse acessar o RDS — o SG do RDS teria que ser reaplicado toda vez que um novo consumidor de rede fosse adicionado em outro repositório.

## Consequências

- Bootstrap de um ambiente novo tem ordem obrigatória (infra-k8s primeiro) — documentada no README do repositório lambda.
- Liberar por CIDR é menos granular que por SG (qualquer recurso nas subnets privadas alcança o RDS na porta 5432, não só EKS/Lambda) — aceitável porque as subnets privadas não têm rota da internet (só via NAT de saída) e não hospedam nada além dos recursos desta aplicação.
