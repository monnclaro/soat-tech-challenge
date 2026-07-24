# ADR 0008 — Prioridade de Custo e AWS Academy

**Status:** Aceito
**Supersede parcialmente:** ADR 0002 (comunicação via API Gateway), ADR 0005 (segredo JWT) — mantém as decisões, muda o mecanismo.

## Contexto

A infraestrutura AWS será operada em uma conta **AWS Academy Learner Lab**, não uma conta AWS comum. Isso muda duas coisas fundamentais em relação ao desenho inicial (Fase 3, primeira versão):

1. **Orçamento fixo e finito** — o Lab tem um teto de gasto definido pela instituição; custo fixo mensal (NAT Gateway, ALB, Secrets Manager) precisa ser eliminado sempre que possível, mesmo que o serviço já estivesse dentro do free tier normal da AWS.
2. **IAM extremamente restrito** — o Academy não permite criar roles, policies ou identity providers IAM (`iam:CreateRole`, `iam:CreateOpenIDConnectProvider`, etc.). Só é possível **reaproveitar** a role já provisionada na conta (`LabRole`). Credenciais também são de **sessão temporária** (expiram em poucas horas), sem suporte a OIDC do GitHub Actions.

## Decisão

| Item | Antes | Agora | Por quê |
|---|---|---|---|
| Saída de rede dos nodes do EKS | NAT Gateway (~$32/mês) | Nodes em **subnet pública**, IP próprio, saída direta pela Internet Gateway | NAT não tem free tier; nodes públicos eliminam o custo sem perder funcionalidade (só piora a postura de segurança — aceitável no contexto de Lab) |
| Exposição da app pro API Gateway | ALB + AWS Load Balancer Controller (~$16-20/mês) | `Service type=NodePort`, API Gateway aponta pro IP público do node | ALB não tem free tier; NodePort é gratuito, ao custo de um IP menos estável (node pode ser substituído) |
| Segredos (credenciais RDS, JWT) | AWS Secrets Manager (~$0,40/segredo/mês) | SSM Parameter Store `SecureString` (chave gerenciada padrão, sem custo) | Secrets Manager cobra por segredo; SSM SecureString cobre o mesmo caso de uso sem custo e sem precisar criar KMS key própria |
| IAM do cluster EKS, node group e Lambdas | Roles criadas pelo Terraform | Reaproveita a `LabRole` já existente (`var.lab_role_arn`, obrigatória, sem default) | Academy bloqueia `iam:CreateRole`/`iam:PutRolePolicy` |
| IRSA (IAM Roles for Service Accounts) | Habilitado (usado pelo ALB Controller) | Desabilitado (`enable_irsa = false`) | Exigiria `iam:CreateOpenIDConnectProvider`, também bloqueado; sem ALB Controller, não há mais consumidor de IRSA |
| Envelope encryption dos Secrets do EKS | KMS key própria (padrão do módulo) | Desabilitada (`cluster_encryption_config = {}`) | `kms:CreateKey` também costuma estar fora da policy do Academy |
| Autenticação do CI/CD com a AWS | OIDC (`aws-actions/configure-aws-credentials` + `role-to-assume`) | Credenciais estáticas de sessão (`AWS_ACCESS_KEY_ID`/`AWS_SECRET_ACCESS_KEY`/`AWS_SESSION_TOKEN` como GitHub Secrets) | OIDC exige criar um IAM Identity Provider (bloqueado); a sessão do Academy já fornece essas três credenciais, só que temporárias |
| Ambientes | `homologacao` + `producao` | Só **`producao`** | Rodar dois ambientes dobraria o custo fixo (2x EKS control plane, 2x RDS) sem necessidade para o exercício |
| Branches | `main` + `homolog` | `master` + `producao` | Reflete o corte de ambiente acima — `master` é a branch de trabalho (PR-only), `producao` aciona o deploy |

Único custo fixo que **não** dá pra remover usando EKS: o control plane (~$0,10/hora). É uma taxa por o cluster existir, sem free tier — inescapável enquanto o requisito for "cluster Kubernetes gerenciado" (EKS).

## Consequências

- **Credenciais de sessão expiram** (tipicamente ~4h) — o CI/CD só funciona enquanto a sessão do Academy Lab estiver ativa; os secrets do GitHub (`AWS_ACCESS_KEY_ID` etc.) precisam ser atualizados manualmente a cada sessão nova. Não há como automatizar isso sem credenciais de longa duração, que o Academy não oferece.
- **IP do node pode mudar**: sem ALB, o API Gateway (repo `lambda`) aponta pro IP público de um node específico do EKS. Se esse node for substituído (scale-down/up, upgrade), o IP muda e o repo `lambda` precisa de um novo `terraform apply` com o IP atualizado — o CI/CD do repo da app republica o parâmetro SSM a cada deploy, mas a integração do API Gateway em si só é atualizada quando o `lambda` reaplica.
- **Nodes com IP público**: pior postura de segurança que nodes em subnet privada atrás de NAT — aceito conscientemente pelo contexto de laboratório/exercício acadêmico, não recomendado para uma carga real de produção.
- **Sem Multi-AZ, sem alta disponibilidade real do banco**: já era o padrão fora de produção; agora é o único modo, já que só existe um ambiente.
- Caminho de reversão: se a conta migrar de AWS Academy para uma conta AWS normal (com IAM e orçamento sem essas restrições), todas as decisões desta ADR podem ser revertidas independentemente umas das outras — reintroduzir NAT, ALB, Secrets Manager e OIDC não exige tocar nas outras.
